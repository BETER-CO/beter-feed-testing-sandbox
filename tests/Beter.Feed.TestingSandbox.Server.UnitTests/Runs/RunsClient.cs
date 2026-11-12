using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using Beter.Feed.TestingSandbox.Server.UnitTests.Hosting;

namespace Beter.Feed.TestingSandbox.Server.UnitTests.Runs;

/// <summary>The run operations of a test host, as a client's test calls them.</summary>
internal static class RunsClient
{
    public static readonly TimeSpan Patience = TimeSpan.FromSeconds(30);

    public static string FixtureRuns => Path.Combine(AppContext.BaseDirectory, "Fixtures", "runs");

    /// <summary>Loads a case of <c>Fixtures/runs</c> through <c>POST /api/cases</c>.</summary>
    public static async Task<int> LoadCaseAsync(SandboxHost host, string fixture)
    {
        var body = await File.ReadAllTextAsync(Path.Combine(FixtureRuns, fixture + ".json"));
        var (status, row) = await SendAsync(host, HttpMethod.Post, "/api/cases", body);
        Assert.True(status == HttpStatusCode.Created, $"POST /api/cases of {fixture} answered {(int)status}: {row}");
        return row.GetProperty("caseId").GetInt32();
    }

    /// <summary>Starts a run and returns its manifest.</summary>
    public static async Task<JsonElement> StartAsync(SandboxHost host, object request)
    {
        var (status, manifest) = await SendAsync(host, HttpMethod.Post, "/api/runs", JsonSerializer.Serialize(request));
        Assert.True(status == HttpStatusCode.Created, $"POST /api/runs answered {(int)status}: {manifest}");
        return manifest;
    }

    public static async Task<JsonElement> GetAsync(SandboxHost host, string runId)
    {
        var (status, manifest) = await SendAsync(host, HttpMethod.Get, $"/api/runs/{runId}", null);
        Assert.Equal(HttpStatusCode.OK, status);
        return manifest;
    }

    /// <summary>Polls a run until it reaches a terminal status, and checks it is the expected one.</summary>
    public static async Task<JsonElement> WaitForStatusAsync(SandboxHost host, string runId, string status, TimeSpan patience)
    {
        var watch = Stopwatch.StartNew();
        while (true)
        {
            var manifest = await GetAsync(host, runId);
            var current = manifest.GetProperty("status").GetString();
            if (current == status || current is "completed" or "stopped" or "failed" || watch.Elapsed > patience)
            {
                Assert.True(current == status, $"Run {runId} is {current}, not {status}: {manifest}");
                return manifest;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(25));
        }
    }

    public static async Task<(HttpStatusCode status, JsonElement body)> SendAsync(SandboxHost host, HttpMethod method, string path, string body)
    {
        using var request = new HttpRequestMessage(method, new Uri(path, UriKind.Relative));
        if (body is not null)
        {
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }

        using var response = await host.Client.SendAsync(request);
        var text = await response.Content.ReadAsStringAsync();
        if (text.Length == 0)
        {
            return (response.StatusCode, default);
        }

        using var document = JsonDocument.Parse(text);
        return (response.StatusCode, document.RootElement.Clone());
    }

    public static DateTimeOffset Instant(JsonElement manifest, string field) =>
        DateTimeOffset.Parse(manifest.GetProperty(field).GetString()!, System.Globalization.CultureInfo.InvariantCulture);
}
