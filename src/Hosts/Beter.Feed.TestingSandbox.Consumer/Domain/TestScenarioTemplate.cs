using Beter.Feed.TestingSandbox.Common.Enums;
using System.Text.Json.Nodes;

namespace Beter.Feed.TestingSandbox.Consumer.Domain
{
    public sealed class TestScenarioTemplate
    {
        public Dictionary<HubKind, Queue<JsonNode>> Messages { get; set; } = new();
        public Dictionary<HubKind, List<TestScenarioTemplateMissmatchItem>> MissmatchItems { get; set; } = new();

        public bool IsProcessed => !Messages.Values.SelectMany(x => x).Any();
        public bool IsFailed => MissmatchItems.Any();
    }
}
