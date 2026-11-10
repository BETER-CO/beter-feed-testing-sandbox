using System.Diagnostics;

namespace Beter.Feed.TestingSandbox.EndToEndTests;

/// <summary>
/// One container of the image under test, started the way the quickstart starts it:
/// <c>docker run -p &lt;port&gt;:51858 &lt;image&gt;</c>. The image is named by the
/// <c>SANDBOX_IMAGE</c> environment variable.
/// </summary>
public sealed class SandboxContainer : IAsyncLifetime
{
    public const string ImageVariable = "SANDBOX_IMAGE";
    public const string VersionVariable = "SANDBOX_VERSION";

    private const int ContainerPort = 51858;
    private static readonly TimeSpan StartPatience = TimeSpan.FromSeconds(90);

    private string? _containerId;

    /// <summary>Gets the version the image was built with, when the build states it.</summary>
    public static string? ExpectedVersion => Environment.GetEnvironmentVariable(VersionVariable) is { Length: > 0 } version
        ? version
        : null;

    public Uri BaseAddress { get; private set; } = null!;

    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var image = Environment.GetEnvironmentVariable(ImageVariable);
        if (string.IsNullOrWhiteSpace(image))
        {
            throw new InvalidOperationException(
                $"Set {ImageVariable} to the image to test, for example {ImageVariable}=beter-feed-testing-sandbox:local.");
        }

        _containerId = (await Docker("run", "--detach", "--publish", $"127.0.0.1::{ContainerPort}", image)).Trim();
        var binding = (await Docker("port", _containerId, $"{ContainerPort}/tcp"))
            .Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0]
            .Trim();
        BaseAddress = new Uri($"http://127.0.0.1:{binding[(binding.LastIndexOf(':') + 1)..]}");
        Client = new HttpClient { BaseAddress = BaseAddress, Timeout = TimeSpan.FromSeconds(10) };

        await WaitUntilHealthy();
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();
        if (_containerId is not null)
        {
            await Docker("rm", "--force", _containerId);
        }
    }

    private static async Task<string> Docker(params string[] arguments)
    {
        var start = new ProcessStartInfo("docker")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        using var process = Process.Start(start) ?? throw new InvalidOperationException("docker did not start.");
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        await process.WaitForExitAsync(timeout.Token);
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"docker {string.Join(' ', arguments)} exited {process.ExitCode}: {await error}");
        }

        return await output;
    }

    private async Task WaitUntilHealthy()
    {
        var watch = Stopwatch.StartNew();
        Exception? last = null;
        while (watch.Elapsed < StartPatience)
        {
            try
            {
                using var response = await Client.GetAsync("/api/health");
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or IOException)
            {
                last = exception;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500));
        }

        var logs = _containerId is null ? string.Empty : await Docker("logs", "--tail", "50", _containerId);
        throw new TimeoutException(
            $"The container did not answer /api/health within {StartPatience.TotalSeconds} s. {last?.Message}{Environment.NewLine}{logs}");
    }
}
