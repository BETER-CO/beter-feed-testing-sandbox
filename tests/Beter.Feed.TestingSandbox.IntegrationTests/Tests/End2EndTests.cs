using Beter.Feed.TestingSandbox.Generator.Contracts.Requests;
using Beter.Feed.TestingSandbox.IntegrationTests.Helpers;
using Beter.Feed.TestingSandbox.IntegrationTests.HttpClients.Abstract;
using Beter.Feed.TestingSandbox.IntegrationTests.Infrastructure;
using Ductus.FluentDocker.Model.Common;
using Ductus.FluentDocker.Model.Compose;
using Ductus.FluentDocker.Services;
using Ductus.FluentDocker.Services.Impl;
using Xunit.Abstractions;

namespace Beter.Feed.TestingSandbox.IntegrationTests.Tests
{
    public class End2EndTests : DockerComposeTestBase
    {
        private readonly ITestOutputHelper _output;
        private readonly IConsumerServiceHttpClient _consumerHttpClient;
        private readonly IGeneratorServiceHttpClient _generatorHttpClient;

        public End2EndTests(ITestOutputHelper output, IConsumerServiceHttpClient consumerHttpClient, IGeneratorServiceHttpClient generatorHttpClient)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
            _consumerHttpClient = consumerHttpClient ?? throw new ArgumentNullException(nameof(consumerHttpClient));
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
            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources/1.json");
            var fileContent = File.ReadAllBytes(directoryPath);

            await _generatorHttpClient.WaitForServiceReadiness();
            await _generatorHttpClient.LoadTestScenario(fileContent, CancellationToken.None);

            await _consumerHttpClient.WaitForServiceReadiness();
            await _consumerHttpClient.LoadTestScenario(fileContent, CancellationToken.None);

            var request = new StartPlaybackRequest()
            {
                CaseId = 1,
                TimeOffsetAfterFirstTimetableMessageInSecounds = 0,
                TimeOffsetInMinutes = 0
            };

            //Act
            await _generatorHttpClient.RunTestScenario(request, CancellationToken.None);

            //Assert
            await WaitHelper.WaitForCondition(async () =>
            {
                var response = await _consumerHttpClient.GetTemplate();

                int unprocessedMessageCount = response.Messages.SelectMany(x => x.Value).Count();
                _output.WriteLine($"Remaining unprocessed messages: {unprocessedMessageCount}");

                return response.IsProcessed;
            });

            var response = await _consumerHttpClient.GetTemplate();

            Assert.False(response.IsFailed);
            Assert.True(response.IsProcessed);
        }
    }
}
