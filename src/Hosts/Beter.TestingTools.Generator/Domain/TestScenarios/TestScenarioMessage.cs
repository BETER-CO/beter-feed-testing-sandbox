using Beter.TestingTools.Generator.Application.Services.Playbacks.Transformations.Helpers;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Beter.TestingTools.Generator.Domain.TestScenarios;

public class TestScenarioMessage<TValue>
{
    public TValue Value { get; set; }
    public string Channel { get; set; }
    public string MessageType { get; set; }
    public long ScheduledAt { get; set; }
}

public sealed class TestScenarioMessage : TestScenarioMessage<JsonNode>
{
    public IEnumerable<FeedMessageWrapper> ToFeedMessages()
    {
        return Value.AsArray().Select(message => new FeedMessageWrapper(message));
    }

    public bool IsMessageType(string messageType)
    {
        return MessageType.Equals(messageType, StringComparison.OrdinalIgnoreCase);
    }

    public TValue Modify<TValue>(Action<TValue> modify)
    {
        var value = GetValue<TValue>();
        modify(value);
        ChangeValue(value);

        return value;
    }

    public TValue GetValue<TValue>() => Value.Deserialize<TValue>();

    private void ChangeValue<TValue>(TValue value)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));

        Value = Serialize(value);
    }

    private static JsonNode Serialize<TValue>(TValue value) => JsonNode.Parse(JsonSerializer.Serialize(value));
}
