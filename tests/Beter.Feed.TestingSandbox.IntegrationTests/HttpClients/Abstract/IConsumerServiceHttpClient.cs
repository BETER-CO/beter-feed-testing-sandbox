using Beter.Feed.TestingSandbox.Consumer.Domain;

namespace Beter.Feed.TestingSandbox.IntegrationTests.HttpClients.Abstract
{
    public interface IConsumerServiceHttpClient : IServiceReadinessChecker
    {
        Task LoadTestScenario(byte[] fileContent, CancellationToken cancellationToken = default);

        Task<TestScenarioTemplate> GetTemplate(CancellationToken cancellationToken = default);
    }
}
