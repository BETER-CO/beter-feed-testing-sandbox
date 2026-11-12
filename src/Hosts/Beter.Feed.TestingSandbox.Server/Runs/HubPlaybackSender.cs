#nullable enable

using Beter.Feed.TestingSandbox.Models.GlobalEvents;
using Beter.Feed.TestingSandbox.Models.Incidents;
using Beter.Feed.TestingSandbox.Models.Scoreboards;
using Beter.Feed.TestingSandbox.Models.TimeTableItems;
using Beter.Feed.TestingSandbox.Models.TradingInfos;
using Beter.Feed.TestingSandbox.Server.Publishers;

namespace Beter.Feed.TestingSandbox.Server.Runs;

/// <summary>Sends through the publishers of the four hubs, to the group of the run's key.</summary>
internal sealed class HubPlaybackSender : IPlaybackSender
{
    private readonly IMessagePublisher<TimeTableItemModel> _timeTable;
    private readonly IMessagePublisher<TradingInfoModel> _trading;
    private readonly IMessagePublisher<ScoreBoardModel> _scoreboard;
    private readonly IMessagePublisher<IncidentModel> _incident;
    private readonly IFeedMessagePublisherResolver _publishers;

    /// <summary>Initializes a new instance of the <see cref="HubPlaybackSender"/> class.</summary>
    /// <param name="timeTable">The publisher of the time table hub.</param>
    /// <param name="trading">The publisher of the trading hub.</param>
    /// <param name="scoreboard">The publisher of the scoreboard hub.</param>
    /// <param name="incident">The publisher of the incident hub.</param>
    /// <param name="publishers">The publishers by channel.</param>
    public HubPlaybackSender(
        IMessagePublisher<TimeTableItemModel> timeTable,
        IMessagePublisher<TradingInfoModel> trading,
        IMessagePublisher<ScoreBoardModel> scoreboard,
        IMessagePublisher<IncidentModel> incident,
        IFeedMessagePublisherResolver publishers)
    {
        _timeTable = timeTable;
        _trading = trading;
        _scoreboard = scoreboard;
        _incident = incident;
        _publishers = publishers;
    }

    /// <inheritdoc />
    public Task SendAsync(Guid apiKey, PlaybackMessage message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        var group = RunGroups.Of(apiKey);
        return message.Payload switch
        {
            TimeTableItemModel[] items => _timeTable.GroupPublish(group, items, cancellationToken),
            TradingInfoModel[] items => _trading.GroupPublish(group, items, cancellationToken),
            ScoreBoardModel[] items => _scoreboard.GroupPublish(group, items, cancellationToken),
            IncidentModel[] items => _incident.GroupPublish(group, items, cancellationToken),
            GlobalMessageModel[] events => _publishers.Resolve(message.Channel).SystemGroupPublish(group, events, cancellationToken),
            string[] ids => _publishers.Resolve(message.Channel).SendGroupRemoveSubscriptionsAsync(group, ids, cancellationToken),
            _ => throw new InvalidOperationException($"No hub method sends a {message.Payload.GetType().Name}."),
        };
    }
}
