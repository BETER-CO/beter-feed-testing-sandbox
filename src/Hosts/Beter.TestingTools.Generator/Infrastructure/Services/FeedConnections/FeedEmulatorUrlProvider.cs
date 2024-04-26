using Beter.TestingTools.Generator.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Beter.TestingTools.Generator.Infrastructure.Services.FeedConnections;

public sealed class FeedEmulatorUrlProvider : IFeedEmulatorUrlProvider
{
    private readonly Uri _baseUrl;

    public FeedEmulatorUrlProvider(IOptions<FeedEmulatorOptions> feedEmulatorOptions)
    {
        var options = feedEmulatorOptions?.Value ?? throw new ArgumentNullException(nameof(feedEmulatorOptions));
        _baseUrl = new Uri(options.ApiBaseUrl);
    }

    public Uri DropConnection(string connectionId) => new(_baseUrl, $"api/connections/{connectionId}");

    public Uri GetConnections() => new(_baseUrl, "api/connections");
}


