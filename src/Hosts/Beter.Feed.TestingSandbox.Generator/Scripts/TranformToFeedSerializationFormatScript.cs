using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Common.Serialization;
using Beter.Feed.TestingSandbox.Generator.Application.Contracts.TestScenarios;
using Beter.Feed.TestingSandbox.Generator.Application.Extensions;
using Beter.Feed.TestingSandbox.Generator.Domain;
using Beter.Feed.TestingSandbox.Generator.Domain.TestScenarios;
using Beter.Feed.TestingSandbox.Models;
using Beter.Feed.TestingSandbox.Models.GlobalEvents;
using Beter.Feed.TestingSandbox.Models.Incidents;
using Beter.Feed.TestingSandbox.Models.Scoreboards;
using Beter.Feed.TestingSandbox.Models.TimeTableItems;
using Beter.Feed.TestingSandbox.Models.TradingInfos;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Beter.Feed.TestingSandbox.Generator.Scripts
{
    public sealed class TranformToFeedSerializationFormatScript : IExecutableScript
    {
        private readonly ITestScenarioFactory _factory;

        public TranformToFeedSerializationFormatScript(ITestScenarioFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public async Task Run()
        {
            var testScenarios = _factory.Create(TestScenario.ResourcesPath);

            foreach (var testScenario in testScenarios)
            {
                var fileContent = TransformToNewFormat(testScenario);
                await WriteToFile(fileContent, testScenario.CaseId.ToString());
            }
        }

        private static async Task WriteToFile(string fileContent, string fileName)
        {
            var testScenarioFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"{fileName}.json");
            await File.WriteAllTextAsync(testScenarioFilePath, fileContent);
        }

        private string TransformToNewFormat(TestScenario testScenario)
        {
            foreach (var message in testScenario.Messages)
            {
                dynamic model;
                switch (message.MessageType)
                {
                    case MessageTypes.SteeringCommand:
                        model = message.Value.Deserialize<SteeringCommand>();
                        message.Value = JsonNode.Parse(JsonHubSerializer.Serialize(model));
                        break;
                    case MessageTypes.SubscriptionsRemoved:
                        model = message.Value.Deserialize<SubscriptionsRemovedModel>();
                        message.Value = JsonNode.Parse(JsonHubSerializer.Serialize(model));
                        break;
                    case MessageTypes.SystemEvent:
                        model = message.Value.Deserialize<IEnumerable<GlobalMessageModel>>();
                        message.Value = JsonNode.Parse(JsonHubSerializer.Serialize(model));
                        break;
                    case MessageTypes.Incident:
                        model = message.Value.Deserialize<IEnumerable<IncidentModel>>();
                        message.Value = JsonNode.Parse(JsonHubSerializer.Serialize(model));
                        break;
                    case MessageTypes.Scoreboard:
                        model = message.Value.Deserialize<IEnumerable<ScoreBoardModel>>();
                        message.Value = JsonNode.Parse(JsonHubSerializer.Serialize(model));
                        break;
                    case MessageTypes.Trading:
                        var tradingInfo = message.Value.Deserialize<IEnumerable<TradingInfoModel>>();

                        foreach (var info in tradingInfo)
                        {
                            foreach (var market in info.Markets)
                            {
                                foreach (var outcome in market.Outcomes.Where(x => x.Prices != null))
                                {
                                    var availableKeys = outcome.Prices.Keys;

                                    outcome.Prices = outcome.Price.ToAllFormats().Where(x => availableKeys.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
                                }
                            }
                        }
                        message.Value = JsonNode.Parse(JsonHubSerializer.Serialize(tradingInfo));
                        break;
                    case MessageTypes.Timetable:
                        model = message.Value.Deserialize<IEnumerable<TimeTableItemModel>>();
                        message.Value = JsonNode.Parse(JsonHubSerializer.Serialize(model));
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            return JsonHubSerializer.Serialize(testScenario);
        }
    }
}
