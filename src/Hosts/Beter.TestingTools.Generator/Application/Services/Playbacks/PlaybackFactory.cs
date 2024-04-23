using Beter.TestingTool.Generator.Application.Contracts;
using Beter.TestingTool.Generator.Application.Contracts.Playbacks;
using Beter.TestingTool.Generator.Application.Contracts.TestScenarios;
using Beter.TestingTool.Generator.Application.Services.Playbacks.Transformations;
using Beter.TestingTool.Generator.Domain.Playbacks;
using Beter.TestingTool.Generator.Domain.TestScenarios;

namespace Beter.TestingTool.Generator.Application.Services.Playbacks;

public sealed class PlaybackFactory : IPlaybackFactory
{
    private readonly ISystemClock _systemClock;
    private readonly ITestScenariosRepository _repository;
    private readonly ITransformationManager _manager;
    private readonly IMessagesTransformationContextFactory _messagesTransformationContextFactory;

    public PlaybackFactory(
        ISystemClock systemClock,
        ITestScenariosRepository repository,
        ITransformationManager manager,
        IMessagesTransformationContextFactory messagesTransformationContextFactory)
    {
        _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        _messagesTransformationContextFactory = messagesTransformationContextFactory ?? throw new ArgumentNullException(nameof(messagesTransformationContextFactory));
    }

    public Playback Create(
        int caseId,
        ReplyMode replyMode,
        int timeOffsetInMinutes,
        int? timeOffsetBetweenMessagesInSecounds,
        int timeOffsetAfterFirstTimetableMessageInSecounds,
        double accelerationFactor)
    {
        var testScenario = _repository.Requre(caseId);
        var playbackId = Guid.NewGuid().ToString();

        var playbackItems = CreatePlaybackItems(
            caseId,
            playbackId,
            replyMode,
            testScenario.Messages,
            TimeSpan.FromMinutes(timeOffsetInMinutes),
            TimeSpan.FromSeconds(timeOffsetAfterFirstTimetableMessageInSecounds),
            accelerationFactor);

        var playback = new Playback
        {
            Id = playbackId,
            CaseId = testScenario.CaseId,
            Description = testScenario.Description,
            StartedAt = _systemClock.UtcNow.AddMinutes(timeOffsetInMinutes).UtcDateTime,
            Version = testScenario.Version,
            Messages = playbackItems.ToDictionary(message => message.InternalId),
            ActiveMessagesCount = playbackItems.Count,
            LastMessageSentAt = 0
        };

        return playback;
    }

    private IReadOnlyCollection<PlaybackItem> CreatePlaybackItems(int caseId, string playbackId, ReplyMode replyMode, IEnumerable<TestScenarioMessage> messages, TimeSpan offset, TimeSpan timeOffsetAfterFirstTimetableMessageInSecounds, double accelerationFactor)
    {
        ArgumentNullException.ThrowIfNull(playbackId, nameof(playbackId));
        ArgumentNullException.ThrowIfNull(messages, nameof(messages));

        var context = _messagesTransformationContextFactory.Create(
            caseId,
            replyMode,
            messages,
            offset,
            timeOffsetAfterFirstTimetableMessageInSecounds,
            accelerationFactor);

        _manager.ApplyTransformation(context, messages);

        var playbackItems = messages.Select(message => new PlaybackItem
        {
            InternalId = Guid.NewGuid().ToString(),
            PlaybackId = playbackId,
            Message = message
        }).ToList();

        return playbackItems.AsReadOnly();
    }
}

