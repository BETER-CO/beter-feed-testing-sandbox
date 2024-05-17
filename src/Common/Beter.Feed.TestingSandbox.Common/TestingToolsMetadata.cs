using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Models;
using Beter.Feed.TestingSandbox.Models.Scoreboards;
using Beter.Feed.TestingSandbox.Models.Incidents;
using Beter.Feed.TestingSandbox.Models.TradingInfos;
using Beter.Feed.TestingSandbox.Models.GlobalEvents;
using Beter.Feed.TestingSandbox.Models.TimeTableItems;

namespace Beter.Feed.TestingSandbox.Common;

public class TestingToolsMetadata
{
    public readonly static MetadataItem Scoreboard = new MetadataItem(MessageTypes.Scoreboard, typeof(ScoreBoardModel));
    public readonly static MetadataItem Incident = new MetadataItem(MessageTypes.Incident, typeof(IncidentModel));
    public readonly static MetadataItem TimeTable = new MetadataItem(MessageTypes.Timetable, typeof(TimeTableItemModel));
    public readonly static MetadataItem Trading = new MetadataItem(MessageTypes.Trading, typeof(TradingInfoModel));
    public readonly static MetadataItem SubscriptionsRemoved = new MetadataItem(MessageTypes.SubscriptionsRemoved, typeof(SubscriptionsRemovedModel));
    public readonly static MetadataItem SystemEvent = new MetadataItem(MessageTypes.SystemEvent, typeof(GlobalMessageModel));
    public readonly static MetadataItem Heartbeat = new MetadataItem(MessageTypes.Heartbeat, typeof(HeartbeatModel));

    public static string ToMessageType<TValue>()
    {
        return GetAll().FirstOrDefault(x => x.ModelType == typeof(TValue)).MessageType;
    }

    public static Type GetModelType(string messageType)
    {
        if (!IsSupported(messageType))
        {
            throw new ArgumentException($"Unsupported message type: {messageType}.");
        }

        var item = GetAll().FirstOrDefault(item => item.MessageType.ToUpperInvariant() == messageType.ToUpperInvariant());

        return item.ModelType;
    }

    public static string GetMessageType(Type modelType)
    {
        var item = GetAll().FirstOrDefault(item => item.ModelType == modelType);
        if (item != null)
        {
            return item.MessageType;
        }

        if (modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            Type elementType = modelType.GetGenericArguments()[0];
            item = GetAll().FirstOrDefault(item => item.ModelType == elementType);
            if (item != null)
            {
                return item.MessageType;
            }
        }

        throw new ArgumentException($"Unsupported model type: {modelType}.");
    }

    public static bool IsSupported(string messageType) => GetAll().FirstOrDefault(item => item.MessageType.ToUpperInvariant() == messageType.ToUpperInvariant()) != null;

    public static IEnumerable<MetadataItem> GetAll()
    {
        yield return Scoreboard;
        yield return Incident;
        yield return TimeTable;
        yield return Trading;
        yield return SubscriptionsRemoved;
        yield return SystemEvent;
        yield return Heartbeat;
    }
}

public sealed class MetadataItem
{
    public string MessageType { get; }
    public Type ModelType { get; }

    public MetadataItem(string messageType, Type modelType)
    {
        MessageType = messageType;
        ModelType = modelType;
    }
}
