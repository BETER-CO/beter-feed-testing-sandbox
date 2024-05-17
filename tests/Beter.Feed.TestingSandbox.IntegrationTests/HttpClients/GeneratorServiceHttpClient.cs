using Beter.Feed.TestingSandbox.Generator.Contracts.Requests;
using Beter.Feed.TestingSandbox.Generator.Domain.Playbacks;
using Beter.Feed.TestingSandbox.IntegrationTests.HttpClients.Abstract;
using System.Text.Json;

namespace Beter.Feed.TestingSandbox.IntegrationTests.HttpClients
{
    public sealed class GeneratorServiceHttpClient : BaseHttpClient, IGeneratorServiceHttpClient
    {
        private readonly IGeneratorServiceUrlProvider _urlProvider;

        public GeneratorServiceHttpClient(IGeneratorServiceUrlProvider urlProvider) : base(urlProvider?.BaseUrl())
        {
            _urlProvider = urlProvider ?? throw new ArgumentNullException(nameof(urlProvider));
        }

        public async Task LoadTestScenario(byte[] fileContent, CancellationToken cancellationToken)
        {
            if (fileContent is null)
                throw new ArgumentNullException(nameof(fileContent));

            var byteArrayContent = new ByteArrayContent(fileContent);
            var multipartContent = new MultipartFormDataContent
            {
                { byteArrayContent, "file", "1" }
            };

            var requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = _urlProvider.LoadTestScenario(),
                Content = multipartContent
            };

            await SendRequest(requestMessage, cancellationToken);
        }

        public async Task<Playback> RunTestScenario(StartPlaybackRequest request, CancellationToken cancellationToken = default)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = _urlProvider.RunTestScenario(),
                Content = MapToContent(request)
            };

            var response = await SendRequest(requestMessage, cancellationToken);

            return JsonSerializer.Deserialize<Playback>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }
}
