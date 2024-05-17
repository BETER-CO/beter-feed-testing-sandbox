using Beter.Feed.TestingSandbox.Common.Models;
using Beter.Feed.TestingSandbox.Generator.Application.Contracts.FeedConnections;

namespace Beter.Feed.TestingSandbox.Generator.Infrastructure.Services.FeedConnections;

public sealed class FeedConnectionService : IFeedConnectionService
{
    private IContentDeserializer _deserializer;
    public IContentDeserializer Deserializer
    {
        get => _deserializer ??= new JsonContentDeserializer();
        set => _deserializer = value;
    }

    private readonly HttpClient _httpClient;
    private readonly IFeedEmulatorUrlProvider _urlProvider;
    private readonly ILogger<FeedConnectionService> _logger;

    public FeedConnectionService(HttpClient httpClient, IFeedEmulatorUrlProvider urlProvider, ILogger<FeedConnectionService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _urlProvider = urlProvider ?? throw new ArgumentNullException(nameof(urlProvider));
    }

    public async Task<IEnumerable<FeedConnection>> GetAsync(CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(nameof(cancellationToken));

        var requestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = _urlProvider.GetConnections()
        };

        var response = await SendRequest(requestMessage, cancellationToken);

        return Deserializer.DeserializeOrThrow<IEnumerable<FeedConnection>>(response);
    }

    public async Task DropConnectionAsync(string connectionId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(connectionId, nameof(connectionId));

        var requestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = _urlProvider.DropConnection(connectionId)
        };

        await SendRequest(requestMessage, cancellationToken);

        _logger.LogInformation($"Connection '{connectionId}' has been dropped.");
    }

    private async Task<string> SendRequest(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            ThrowBadRequestException(request.RequestUri);
        }

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private static void ThrowBadRequestException(Uri requestUri) =>
        throw new BadRequestException($"FeedEmulatorClient. Unknown error occured during processing uri: {requestUri}.");
}

public class BadRequestException : Exception
{
    public BadRequestException()
    {
    }

    public BadRequestException(string message)
        : base(message)
    {
    }

    public BadRequestException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
