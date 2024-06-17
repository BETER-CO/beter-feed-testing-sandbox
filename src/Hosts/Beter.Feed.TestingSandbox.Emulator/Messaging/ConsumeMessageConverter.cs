using Confluent.Kafka;
using Microsoft.Extensions.Logging.Abstractions;
using Beter.Feed.TestingSandbox.Common.Serialization;
using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Models;
using Beter.Feed.TestingSandbox.Models.Scoreboards;
using Beter.Feed.TestingSandbox.Models.Incidents;
using Beter.Feed.TestingSandbox.Models.TradingInfos;
using Beter.Feed.TestingSandbox.Models.GlobalEvents;
using Beter.Feed.TestingSandbox.Models.TimeTableItems;
using Beter.Feed.TestingSandbox.Emulator.Messaging.Extensions;

namespace Beter.Feed.TestingSandbox.Emulator.Messaging;

public sealed class ConsumeMessageConverter : IConsumeMessageConverter
{
    private readonly ILogger<ConsumeMessageConverter> _logger;

    private readonly static Dictionary<string, Type> KnownTypes;

    public ConsumeMessageConverter(ILogger<ConsumeMessageConverter> logger)
    {
        _logger = logger ?? NullLogger<ConsumeMessageConverter>.Instance;
    }

    static ConsumeMessageConverter()
    {
        KnownTypes = new Dictionary<string, Type>
        {
            { MessageTypes.Scoreboard.ToUpperInvariant(), typeof(ScoreBoardModel[]) },
            { MessageTypes.Trading.ToUpperInvariant(), typeof(TradingInfoModel[]) },
            { MessageTypes.Timetable.ToUpperInvariant(), typeof(TimeTableItemModel[]) },
            { MessageTypes.Incident.ToUpperInvariant(), typeof(IncidentModel[]) },
            { MessageTypes.SubscriptionsRemoved.ToUpperInvariant(), typeof(SubscriptionsRemovedModel) },
            { MessageTypes.SystemEvent.ToUpperInvariant(), typeof(GlobalMessageModel[]) },
            { MessageTypes.SteeringCommand.ToUpperInvariant(), typeof(SteeringCommandModel) }
        };
    }

    public bool CanProcess(ConsumeResult<byte[], byte[]> consumeResult)
    {
        var typeHeader = consumeResult.Message.Headers.FirstOrDefault(x => x.Key == HeaderNames.MessageType);
        if (typeHeader == null)
        {
            return false;
        }

        var type = typeHeader.GetValueBytes().ToUtf8String()?.ToUpperInvariant();
        return type != null && KnownTypes.TryGetValue(type, out _);
    }

    public ConsumeMessageContext ConvertToMessageContextFromConsumeResult(ConsumeResult<byte[], byte[]> consumeResult)
    {
        var headers = new Dictionary<string, string>();
        foreach (var header in consumeResult.Message.Headers)
        {
            headers[header.Key] = header.GetValueBytes().ToUtf8String()!;
        }

        var messageTypeString = headers.GetValueOrDefault(HeaderNames.MessageType, MessageTypes.Timetable);
        var messageType = KnownTypes[messageTypeString.ToUpperInvariant()];
        var messageValue = consumeResult.Message.Value.ToUtf8String();

        _logger.LogInformation($"Body: {messageValue}");

        var messageTypedInstance = JsonHubSerializer.Deserialize(messageValue, messageType);
        var messageContext = new ConsumeMessageContext
        {
            MessageHeaders = headers,
            MessageType = messageType,
            MessageObject = messageTypedInstance,
        };

        return messageContext;
    }
}
