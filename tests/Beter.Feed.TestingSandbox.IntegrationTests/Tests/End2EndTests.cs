using AutoFixture;
using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Common.Enums;
using Beter.Feed.TestingSandbox.Common.Extensions;
using Beter.Feed.TestingSandbox.Common.Serialization;
using Beter.Feed.TestingSandbox.Emulator.Messaging;
using Beter.Feed.TestingSandbox.Generator.Application.Contracts.TestScenarios;
using Beter.Feed.TestingSandbox.Generator.Contracts.Requests;
using Beter.Feed.TestingSandbox.IntegrationTests.Helpers;
using Beter.Feed.TestingSandbox.IntegrationTests.HttpClients.Abstract;
using Beter.Feed.TestingSandbox.IntegrationTests.Infrastructure;
using Beter.Feed.TestingSandbox.IntegrationTests.Infrastructure.Kafka;
using Beter.Feed.TestingSandbox.IntegrationTests.Options;
using Beter.Feed.TestingSandbox.IntegrationTests.Services;
using Beter.Feed.TestingSandbox.Models;
using Beter.Feed.TestingSandbox.Models.GlobalEvents;
using Beter.Feed.TestingSandbox.Models.Incidents;
using Beter.Feed.TestingSandbox.Models.Scoreboards;
using Beter.Feed.TestingSandbox.Models.TimeTableItems;
using Beter.Feed.TestingSandbox.Models.TradingInfos;
using Confluent.Kafka;
using Ductus.FluentDocker.Model.Common;
using Ductus.FluentDocker.Model.Compose;
using Ductus.FluentDocker.Services;
using Ductus.FluentDocker.Services.Impl;
using Microsoft.Extensions.Options;
using Xunit.Abstractions;

namespace Beter.Feed.TestingSandbox.IntegrationTests.Tests
{
    public class End2EndTests : DockerComposeTestBase
    {
        private static readonly Fixture Fixture = new();

        private readonly ITestOutputHelper _output;
        private readonly ConsumerOptions _consumerOptions;
        private readonly IConsumeMessageConverter _consumeMessageConverter;
        private readonly ITestScenarioFactory _testScenarioFactory;
        private readonly IFeedDataService _feedDataService;
        private readonly IGeneratorServiceHttpClient _generatorHttpClient;

        public End2EndTests(
            ITestOutputHelper output,
            IOptions<ConsumerOptions> consumerOptions,
            IConsumeMessageConverter consumeMessageConverter,
            ITestScenarioFactory testScenarioFactory,
            IFeedDataService feedDataService,
            IGeneratorServiceHttpClient generatorHttpClient)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
            _consumeMessageConverter = consumeMessageConverter ?? throw new ArgumentNullException(nameof(consumeMessageConverter));
            _consumerOptions = consumerOptions?.Value ?? throw new ArgumentNullException(nameof(consumerOptions));
            _testScenarioFactory = testScenarioFactory ?? throw new ArgumentNullException(nameof(testScenarioFactory));
            _feedDataService = feedDataService ?? throw new ArgumentNullException(nameof(feedDataService));
            _generatorHttpClient = generatorHttpClient ?? throw new ArgumentNullException(nameof(generatorHttpClient));
        }

        protected override ICompositeService Build()
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), (TemplateString)"docker-compose.tests.yml");

            return new DockerComposeCompositeService(
                DockerHost,
                new DockerComposeConfig
                {
                    ComposeFilePath = new List<string> { file },
                    ForceBuild = true,
                    ForceRecreate = true,
                    RemoveOrphans = true,
                    StopOnDispose = true
                });
        }

        [Fact]
        public async Task GenerateAndConsumeData_Successfully()
        {
            //Arrange
            var fileContent = GetFileContent("Resources/1.json");
            await LoadTestScenarioForGeneratorService(fileContent);

            var request = Fixture.Build<StartPlaybackRequest>()
                  .With(x => x.CaseId, 1)
                  .With(x => x.TimeOffsetInMinutes, 0)
                  .With(x => x.TimeOffsetAfterFirstTimetableMessageInSecounds, 0)
                  .Create();

            //Act
            await _generatorHttpClient.RunTestScenario(request, CancellationToken.None);

            //Assert
            var expected = await _testScenarioFactory.Create(1, new MemoryStream(fileContent));
            _feedDataService.SetExpectedData(expected);

            using (var consumer = StartFeedDataConsuming())
            {
                await AssertFeedDataProcessingComplete();
            }
        }
        
        private KafkaConsumer StartFeedDataConsuming()
        {
            var kafkaConsumer = new KafkaConsumer(_consumerOptions);
            kafkaConsumer.StartConsuming(KafkaTopics.FeedTestingSandboxConsumer, VerifyAndCompareMessage);

            return kafkaConsumer;
        }

        private async Task AssertFeedDataProcessingComplete()
        {
            await WaitHelper.WaitForCondition(() =>
            {
                var expected = _feedDataService.GetExpectedData();
                var unprocessedMessageCount = expected.Messages.SelectMany(x => x.Value).Count();

                _output.WriteLine($"Remaining unprocessed messages: {unprocessedMessageCount}");

                return expected.IsProcessed || expected.IsFailed;
            });

            var result = _feedDataService.GetExpectedData();

            Assert.False(result.IsFailed);
            Assert.True(result.IsProcessed);
        }

        private static byte[] GetFileContent(string relativePath)
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);
            return File.ReadAllBytes(fullPath);
        }

        private async Task LoadTestScenarioForGeneratorService(byte[] fileContent)
        {
            await _generatorHttpClient.WaitForServiceReadiness();
            await _generatorHttpClient.LoadTestScenario(fileContent, CancellationToken.None);
        }

        private void VerifyAndCompareMessage(ConsumeResult<byte[], byte[]> consumeResult)
        {
            var context = _consumeMessageConverter.ConvertToMessageContextFromConsumeResult(consumeResult);
            var messageType = context.MessageHeaders[HeaderNames.MessageType];
            var channel = context.MessageHeaders[HeaderNames.MessageChannel];

            switch (messageType)
            {
                case MessageTypes.Scoreboard:
                    EnsureReceivedExpectedFeedMessage(channel, (ScoreBoardModel[])context.MessageObject);
                    break;
                case MessageTypes.Trading:
                    EnsureReceivedExpectedFeedMessage(channel, (TradingInfoModel[])context.MessageObject);
                    break;
                case MessageTypes.Incident:
                    EnsureReceivedExpectedFeedMessage(channel, (IncidentModel[])context.MessageObject);
                    break;
                case MessageTypes.Timetable:
                    EnsureReceivedExpectedFeedMessage(channel, (TimeTableItemModel[])context.MessageObject);
                    break;
                case MessageTypes.SubscriptionsRemoved:
                    EnsureReceivedExpectedFeedMessage(channel, (SubscriptionsRemovedModel)context.MessageObject);
                    break;
                case MessageTypes.SystemEvent:
                    EnsureReceivedExpectedFeedMessage(channel, (GlobalMessageModel[])context.MessageObject);
                    break;
            }
        }

        private void EnsureReceivedExpectedFeedMessage<TValue>(string channel, TValue actual) where TValue : class
        {
            var hubKind = HubEnumHelper.ToHub(channel);
            _feedDataService.TryGetNext(hubKind, out var nextItem);
            var expected = JsonHubSerializer.Deserialize<TValue>(nextItem);

            if(!FeedDataMessagesComparer.Compare(expected, actual))
            {
                LogMessageMismatch(expected, actual, hubKind);
            }
        }

        private void LogMessageMismatch<TValue>(TValue expectedMessage, TValue actualMessage, HubKind hubKind)
        {
            var actual = JsonHubSerializer.Serialize(actualMessage);
            var expected = JsonHubSerializer.Serialize(expectedMessage);

            _feedDataService.SetMissmatchItem(hubKind, expected, actual);
        }
    }
}
