using Beter.Feed.TestingSandbox.Common.Enums;
using System.Text.Json.Nodes;

namespace Beter.Feed.TestingSandbox.IntegrationTests.Services
{
    public sealed class FeedData
    {
        public Dictionary<HubKind, Queue<JsonNode>> Messages { get; set; } = new();
        public Dictionary<HubKind, List<FeedDataMissmatchItem>> MissmatchItems { get; set; } = new();

        public bool IsProcessed => !Messages.Values.SelectMany(x => x).Any();
        public bool IsFailed => MissmatchItems.Any();
    }

    public sealed class FeedDataMissmatchItem
    {
        public string Actual { get; set; }
        public string Expected { get; set; }

        public FeedDataMissmatchItem(string expected, string actual)
        {
            Actual = actual ?? throw new ArgumentNullException(nameof(actual));
            Expected = expected ?? throw new ArgumentNullException(nameof(expected));
        }
    }
}
