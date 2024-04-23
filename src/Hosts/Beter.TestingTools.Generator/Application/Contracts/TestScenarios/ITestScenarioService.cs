using Beter.TestingTool.Generator.Contracts.Playbacks;
using Beter.TestingTool.Generator.Contracts.Requests;
using Beter.TestingTool.Generator.Contracts.Responses;
using Beter.TestingTool.Generator.Contracts.TestScenarios;

namespace Beter.TestingTool.Generator.Application.Contracts.TestScenarios;

public interface ITestScenarioService
{
    IEnumerable<PlaybackDto> GetActivePlaybacks();

    IReadOnlyDictionary<int, TestScenarioDto> GetAll();

    PlaybackDto Start(StartPlaybackRequest request);

    StopPlaybackResponse Stop(StopPlaybackRequest request);
}