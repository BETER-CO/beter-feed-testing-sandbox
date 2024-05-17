using Beter.Feed.TestingSandbox.IntegrationTests.HttpClients.Abstract;
using Beter.Feed.TestingSandbox.IntegrationTests.Options;
using Microsoft.Extensions.Options;

namespace Beter.Feed.TestingSandbox.IntegrationTests.HttpClients
{
    public sealed class ConsumerServiceUrlProvider : IConsumerServiceUrlProvider
    {
        private readonly Uri _baseUrl;

        public ConsumerServiceUrlProvider(IOptions<HttpClientsOptions> httpClientsOptions)
        {
            _baseUrl = new Uri(httpClientsOptions.Value.ConsumerServiceHost);
        }

        public Uri BaseUrl() => _baseUrl;
        public Uri LoadTestScenario() => new Uri(_baseUrl, "/api/test-scenario-templates/load");
        public Uri GetTemplate() => new Uri(_baseUrl, "/api/test-scenario-templates");
    }
}
