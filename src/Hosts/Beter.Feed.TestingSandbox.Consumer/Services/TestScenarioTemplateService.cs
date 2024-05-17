using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Common.Enums;
using Beter.Feed.TestingSandbox.Common.Extensions;
using Beter.Feed.TestingSandbox.Consumer.Domain;
using Beter.Feed.TestingSandbox.Consumer.Services.Abstract;
using System.Text.Json.Nodes;

namespace Beter.Feed.TestingSandbox.Consumer.Services
{
    public sealed class TestScenarioTemplateService : ITestScenarioTemplateService
    {
        private TestScenarioTemplate _template;
        private static object _lock = new();

        public JsonNode GetNext(HubKind hubKind)
        {
            lock (_lock)
            {
                var template = GetTemplate();
                if (!template.Messages.TryGetValue(hubKind, out var queue))
                {
                    throw new ArgumentException($"Cannot get messages from channel: {hubKind}.");
                }

                return queue.Dequeue();
            }
        }

        public TestScenarioTemplate GetTemplate()
        {
            lock (_lock)
            {
                return _template;
            }
        }

        public TestScenarioTemplate SetMissmatchItem(HubKind hubKind, string expected, string actual)
        {
            lock (_lock)
            {
                var template = GetTemplate();
                if (template.MissmatchItems.TryGetValue(hubKind, out var items))
                    items.Add(new TestScenarioTemplateMissmatchItem(expected, actual));
                else
                    template.MissmatchItems.Add(hubKind, new List<TestScenarioTemplateMissmatchItem>() { new TestScenarioTemplateMissmatchItem(expected, actual) });

                return template;
            }
        }

        public TestScenarioTemplate SetTemplate(TestScenario testScenario)
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

                _template = new TestScenarioTemplate { Messages = messages };

                return _template;
            }
        }
    }
}
