using Beter.Feed.TestingSandbox.Generator.Contracts.Playbacks;
using Beter.Feed.TestingSandbox.Generator.Contracts.Requests;
using Beter.Feed.TestingSandbox.Generator.Contracts.Responses;
using Beter.Feed.TestingSandbox.Generator.Contracts.TestScenarios;

namespace Beter.Feed.TestingSandbox.Generator.Application.Contracts.TestScenarios;

public interface ITestScenarioService
{
    IEnumerable<PlaybackDto> GetActivePlaybacks();

    IReadOnlyDictionary<int, TestScenarioDto> GetAll();

    PlaybackDto Start(StartPlaybackRequest request);

    StopPlaybackResponse Stop(StopPlaybackRequest request);
}