using Beter.Feed.TestingSandbox.Generator.Contracts.Requests;
using Beter.Feed.TestingSandbox.Generator.Domain.Playbacks;

namespace Beter.Feed.TestingSandbox.IntegrationTests.HttpClients.Abstract
{
    public interface IGeneratorServiceHttpClient : IServiceReadinessChecker
    {
        Task LoadTestScenario(byte[] fileContent, CancellationToken cancellationToken = default);

        Task<Playback> RunTestScenario(StartPlaybackRequest request, CancellationToken cancellationToken = default);
    }
}
