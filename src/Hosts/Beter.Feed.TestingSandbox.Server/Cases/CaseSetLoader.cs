#nullable enable

using Microsoft.Extensions.Options;

namespace Beter.Feed.TestingSandbox.Server.Cases;

/// <summary>
/// Loads the case set when the host starts. <c>GET /api/health</c> answers <c>503</c> until
/// it is done; a case set that does not load stops the host.
/// </summary>
public sealed class CaseSetLoader : BackgroundService
{
    private readonly CaseCatalogue _catalogue;
    private readonly IOptions<CaseOptions> _options;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<CaseSetLoader> _logger;

    /// <summary>Initializes a new instance of the <see cref="CaseSetLoader"/> class.</summary>
    /// <param name="catalogue">The catalogue to load.</param>
    /// <param name="options">Where the case set is.</param>
    /// <param name="lifetime">The lifetime of the host.</param>
    /// <param name="logger">The logger.</param>
    public CaseSetLoader(
        CaseCatalogue catalogue,
        IOptions<CaseOptions> options,
        IHostApplicationLifetime lifetime,
        ILogger<CaseSetLoader> logger)
    {
        _catalogue = catalogue;
        _options = options;
        _lifetime = lifetime;
        _logger = logger;
    }

    /// <summary>The directory of the case set, as configured.</summary>
    /// <param name="options">Where the case set is.</param>
    /// <returns>The full path of the directory.</returns>
    public static string DirectoryOf(CaseOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return Path.GetFullPath(options.Directory, AppContext.BaseDirectory);
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // The host must be listening, and answering 503, while the case set loads.
        await Task.Yield();

        var directory = DirectoryOf(_options.Value);
        try
        {
            _catalogue.Load(directory);
            if (_logger.IsEnabled(LogLevel.Information))
            {
                var ids = _catalogue.List(Array.Empty<string>(), active: true, retired: true).Count;
                _logger.LogInformation("Case set loaded from {Directory}: {Count} ids.", directory, ids);
            }
        }
#pragma warning disable CA1031 // Whatever stops the load, the sandbox must log it and stop: it cannot serve without its case set.
        catch (Exception exception) when (exception is not OperationCanceledException)
#pragma warning restore CA1031
        {
            _logger.LogCritical(exception, "The case set in {Directory} did not load; the sandbox stops.", directory);
            Environment.ExitCode = 1;
            _lifetime.StopApplication();
        }
    }
}
