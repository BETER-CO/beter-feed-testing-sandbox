using System.Text.Json.Nodes;

namespace Beter.Feed.TestingSandbox.Consumer.Domain
{
    public sealed class TestScenarioMessage
    {
        public JsonNode Value { get; set; }
        public string Channel { get; set; }
        public string MessageType { get; set; }
    }
}
