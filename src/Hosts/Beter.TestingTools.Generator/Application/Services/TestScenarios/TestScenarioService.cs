using Beter.TestingTool.Generator.Application.Contracts.Playbacks;
using Beter.TestingTool.Generator.Application.Contracts.TestScenarios;
using Beter.TestingTool.Generator.Application.Mappers;
using Beter.TestingTool.Generator.Contracts.Playbacks;
using Beter.TestingTool.Generator.Contracts.Requests;
using Beter.TestingTool.Generator.Contracts.Responses;
using Beter.TestingTool.Generator.Contracts.TestScenarios;
using Beter.TestingTool.Generator.Domain.Playbacks;

namespace Beter.TestingTool.Generator.Application.Services.TestScenarios;

public sealed class TestScenarioService : ITestScenarioService
{
    private readonly double _accelerationFactor = 1;

    private readonly IPlaybackFactory _playbackFactory;
    private readonly IPlaybackRepository _playbackRepository;
    private readonly ITestScenariosRepository _testScenariosRepository;
    private readonly ILogger<TestScenarioService> _logger;

    public TestScenarioService(
        IPlaybackFactory playbackFactory,
        IPlaybackRepository playbackRepository,
        ITestScenariosRepository testScenariosRepository,
        ILogger<TestScenarioService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _playbackFactory = playbackFactory ?? throw new ArgumentNullException(nameof(playbackFactory));
        _playbackRepository = playbackRepository ?? throw new ArgumentNullException(nameof(playbackRepository));
        _testScenariosRepository = testScenariosRepository ?? throw new ArgumentNullException(nameof(testScenariosRepository));
    }

    public IEnumerable<PlaybackDto> GetActivePlaybacks()
    {
        var playbacks = _playbackRepository.GetActive();

        return playbacks.Select(PlaybackMapper.MapToDto).ToList();
    }

    public IReadOnlyDictionary<int, TestScenarioDto> GetAll()
    {
        var testScenarios = _testScenariosRepository.GetAll();

        return testScenarios.ToDictionary(
            kv => kv.Key,
            kv => TestScenarioMapper.MapToDto(kv.Value));
    }

    public PlaybackDto Start(StartPlaybackRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var playback = _playbackFactory.Create(
            request.CaseId,
            ReplyMode.HistoricalTimeline, // Stage - 2: request.ReplyMode
            request.TimeOffsetInMinutes,
            null, // Stage - 2: request.TimeOffsetBetweenMessagesInSeconds, 
            request.TimeOffsetAfterFirstTimetableMessageInSecounds,
            _accelerationFactor); // Stage - 2: request.AccelerationFactor

        _playbackRepository.Add(playback);

        _logger.LogInformation($"Schedule playback for test case {request.CaseId} at {playback.StartedAt} with {playback.ActiveMessagesCount} messages.");

        return PlaybackMapper.MapToDto(playback);
    }

    public StopPlaybackResponse Stop(StopPlaybackRequest request)
    {
        IEnumerable<Playback> removedPlaybacks;

        if (request.Command == StopPlaybackCommand.StopSingle)
        {
            removedPlaybacks = [_playbackRepository.Remove(request.PlaybackId)];
        }
        else
        {
            removedPlaybacks = _playbackRepository.RemoveAll();
        }

        var response = new StopPlaybackResponse
        {
            Command = request.Command,
            Items = removedPlaybacks.Select(PlaybackMapper.MapToStopPlaybackItemResponse)
        };

        return response;
    }
}
