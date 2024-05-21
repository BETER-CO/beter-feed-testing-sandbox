using Beter.Feed.TestingSandbox.Generator.Application.Contracts.Playbacks;
using Beter.Feed.TestingSandbox.Generator.Application.Contracts.TestScenarios;
using Beter.Feed.TestingSandbox.Generator.Application.Mappers;
using Beter.Feed.TestingSandbox.Generator.Contracts.Playbacks;
using Beter.Feed.TestingSandbox.Generator.Contracts.Requests;
using Beter.Feed.TestingSandbox.Generator.Contracts.Responses;
using Beter.Feed.TestingSandbox.Generator.Contracts.TestScenarios;
using Beter.Feed.TestingSandbox.Generator.Domain.Playbacks;

namespace Beter.Feed.TestingSandbox.Generator.Application.Services.TestScenarios;

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

    /* TODO:
     * For stage - 2:
     * request.ReplyMode
     * request.TimeOffsetBetweenMessagesInSeconds 
     * request.AccelerationFactor 
     */
    public PlaybackDto Start(StartPlaybackRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var playback = _playbackFactory.Create(
            request.CaseId,
            ReplyMode.HistoricalTimeline,
            request.TimeOffsetInMinutes,
            null,
            request.TimeOffsetAfterFirstTimetableMessageInSecounds,
            _accelerationFactor);

        _playbackRepository.Add(playback);

        _logger.LogInformation($"Schedule playback for test case {request.CaseId} at {playback.StartedAt} with {playback.ActiveMessagesCount} messages.");

        return PlaybackMapper.MapToDto(playback);
    }

    public StopPlaybackResponse Stop(StopPlaybackRequest request)
    {
        IEnumerable<Playback> removedPlaybacks;

        switch (request.Command)
        {
            case StopPlaybackCommand.StopSingle:
                removedPlaybacks = new[] { _playbackRepository.Remove(request.PlaybackId) };
                break;
            case StopPlaybackCommand.StopAll:
                removedPlaybacks = _playbackRepository.RemoveAll();
                break;
            default:
                throw new ArgumentException($"Unsupported command: {request.Command}");
        }

        var response = new StopPlaybackResponse
        {
            Command = request.Command,
            Items = removedPlaybacks.Select(PlaybackMapper.MapToStopPlaybackItemResponse)
        };

        return response;
    }
}
