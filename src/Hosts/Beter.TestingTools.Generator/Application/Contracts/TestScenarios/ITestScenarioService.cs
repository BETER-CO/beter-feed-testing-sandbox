using Beter.TestingTools.Generator.Contracts.Playbacks;
using Beter.TestingTools.Generator.Contracts.Requests;
using Beter.TestingTools.Generator.Contracts.Responses;
using Beter.TestingTools.Generator.Contracts.TestScenarios;

namespace Beter.TestingTools.Generator.Application.Contracts.TestScenarios;

public interface ITestScenarioService
{
    IEnumerable<PlaybackDto> GetActivePlaybacks();

    IReadOnlyDictionary<int, TestScenarioDto> GetAll();

    PlaybackDto Start(StartPlaybackRequest request);

    StopPlaybackResponse Stop(StopPlaybackRequest request);
}