using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Common.Serialization;
using Beter.Feed.TestingSandbox.Generator.Application.Extensions;
using Beter.Feed.TestingSandbox.Generator.Domain;
using Beter.Feed.TestingSandbox.Generator.Domain.TestScenarios;
using Beter.Feed.TestingSandbox.Generator.Host.Common.ApplicationConfiguration.Interfaces;
using Beter.Feed.TestingSandbox.Generator.Host.Common.Constants;
using Beter.Feed.TestingSandbox.Models;
using Beter.Feed.TestingSandbox.Models.GlobalEvents;
using Beter.Feed.TestingSandbox.Models.Incidents;
using Beter.Feed.TestingSandbox.Models.Scoreboards;
using Beter.Feed.TestingSandbox.Models.TimeTableItems;
using Beter.Feed.TestingSandbox.Models.TradingInfos;
using System.Text;
using System.Text.Json.Nodes;

namespace Beter.Feed.TestingSandbox.Generator.Host.Endpoints.TestScenarios
{
    /// <summary>
    /// This endpoint serves a temporary purpose and should be reviewed for potential removal in the near future.
    /// </summary>
    public sealed class TransformTestScenarioFileToFeedSerializationFormat : IEndpointProvider
    {
        public static void DefineEndpoints(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost($"{ApiConstant.ApiPrefix}/test-scenarios/transform", TransformTestScenarioFileToFeedSerializationFormatHandler)
                .WithName("MapTestScenarioFileToFeedSerializationFormat")
                .WithTags(ApiConstant.TestScenarioTag);
        }

        private async static Task<IResult> TransformTestScenarioFileToFeedSerializationFormatHandler(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Results.BadRequest("File is empty.");

            using (var stream = file.OpenReadStream())
            {
                var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                if (!int.TryParse(fileName, out var caseId))
                    return Results.BadRequest("Invalid file name format.");

                using (var streamReader = new StreamReader(stream))
                {
                    var content = await streamReader.ReadToEndAsync();
                    var testScenario = JsonHubSerializer.Deserialize<TestScenario>(content);

                    foreach (var message in testScenario.Messages)
                    {
                        dynamic model;
                        switch (message.MessageType)
                        {
                            case MessageTypes.SteeringCommand:
                                model = JsonHubSerializer.Deserialize<SteeringCommand>(message.Value);
                                message.Value = JsonNode.Parse(JsonHubSerializer.Serialize(model));
                                break;
                            case MessageTypes.SubscriptionsRemoved:
                                model = JsonHubSerializer.Deserialize<SubscriptionsRemovedModel>(message.Value);
                                message.Value = JsonNode.Parse(JsonHubSerializer.Serialize(model));
                                break;
                            case MessageTypes.SystemEvent:
                                model = JsonHubSerializer.Deserialize<IEnumerable<GlobalMessageModel>>(message.Value);
                                message.Value = JsonNode.Parse(JsonHubSerializer.Serialize(model));
                                break;
                            case MessageTypes.Incident:
                                model = JsonHubSerializer.Deserialize<IEnumerable<IncidentModel>>(message.Value);
                                message.Value = JsonNode.Parse(JsonHubSerializer.Serialize(model));
                                break;
                            case MessageTypes.Scoreboard:
                                model = JsonHubSerializer.Deserialize<IEnumerable<ScoreBoardModel>>(message.Value);
                                message.Value = JsonNode.Parse(JsonHubSerializer.Serialize(model));
                                break;
                            case MessageTypes.Trading:
                                var tradingInfo = JsonHubSerializer.Deserialize<IEnumerable<TradingInfoModel>>(message.Value);

                                foreach (var info in tradingInfo)
                                {
                                    foreach (var market in info.Markets)
                                    {
                                        foreach (var outcome in market.Outcomes)
                                        {
                                            var availableKeys = outcome.Prices.Keys;

                                            outcome.Prices = outcome.Price.ToAllFormats().Where(x => availableKeys.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
                                        }
                                    }
                                }
                                message.Value = JsonNode.Parse(JsonHubSerializer.Serialize(tradingInfo));
                                break;
                            case MessageTypes.Timetable:
                                model = JsonHubSerializer.Deserialize<IEnumerable<TimeTableItemModel>>(message.Value);
                                message.Value = JsonNode.Parse(JsonHubSerializer.Serialize(model));
                                break;
                            default:
                                throw new InvalidOperationException();
                        }
                    }

                    var fileContent = Encoding.UTF8.GetBytes(JsonHubSerializer.Serialize(testScenario));

                    return Results.File(
                        fileContent,
                        "application/json",
                        caseId.ToString());
                }
            }
        }
    }
}
