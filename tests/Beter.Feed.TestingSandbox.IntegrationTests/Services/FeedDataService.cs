using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Common.Enums;
using Beter.Feed.TestingSandbox.Common.Extensions;
using System.Text.Json.Nodes;
using TestScenario = Beter.Feed.TestingSandbox.Generator.Domain.TestScenarios.TestScenario;

namespace Beter.Feed.TestingSandbox.IntegrationTests.Services
{
    public interface IFeedDataService
    {
        bool TryGetNext(HubKind hubKind, out JsonNode value);
        FeedData SetMissmatchItem(HubKind hubKind, string expected, string actual);
        FeedData GetExpectedData();
        FeedData SetExpectedData(TestScenario testScenario);
    }

    public sealed class FeedDataService : IFeedDataService
    {
        private FeedData _expected;
        private static readonly object _lock = new();

        public bool TryGetNext(HubKind hubKind, out JsonNode value)
        {
            lock (_lock)
            {
                var expected = GetExpectedData();
                if (expected.Messages.TryGetValue(hubKind, out var queue))
                {
                    if (queue.TryDequeue(out var dequeuedValue))
                    {
                        value = dequeuedValue;
                        return true;
                    }
                }

                value = null;
                return false;
            }
        }

        public FeedData SetMissmatchItem(HubKind hubKind, string expected, string actual)
        {
            lock (_lock)
            {
                var expectedFeedData = GetExpectedData();
                if (expectedFeedData.MissmatchItems.TryGetValue(hubKind, out var items))
                    items.Add(new FeedDataMissmatchItem(expected, actual));
                else
                    expectedFeedData.MissmatchItems.Add(hubKind, new List<FeedDataMissmatchItem>() { new FeedDataMissmatchItem(expected, actual) });

                return expectedFeedData;
            }
        }

        public FeedData GetExpectedData()
        {
            lock (_lock)
            {
                return _expected;
            }
        }

        public FeedData SetExpectedData(TestScenario testScenario)
        {
            ArgumentNullException.ThrowIfNull(testScenario);

            var allMessageTypes = new[]
            {
                MessageTypes.Timetable,
                MessageTypes.Trading,
                MessageTypes.Scoreboard,
                MessageTypes.Incident,
                MessageTypes.SubscriptionsRemoved,
                MessageTypes.SystemEvent
            };

            lock (_lock)
            {
                var messages = testScenario.Messages
                    .Where(x => x.Channel != null && allMessageTypes.Contains(x.MessageType))
                    .GroupBy(x => x.Channel, x => x.Value)
                    .ToDictionary(
                        x => HubEnumHelper.ToHub(x.Key),
                        x => new Queue<JsonNode>(x));

                _expected = new FeedData { Messages = messages };

                return _expected;
            }
        }
    }
}
