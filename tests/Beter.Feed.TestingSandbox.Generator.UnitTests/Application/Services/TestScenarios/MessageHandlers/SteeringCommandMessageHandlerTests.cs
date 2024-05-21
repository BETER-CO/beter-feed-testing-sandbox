using AutoFixture;
using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Common.Models;
using Beter.Feed.TestingSandbox.Generator.Application.Contracts.Heartbeats;
using Beter.Feed.TestingSandbox.Generator.Application.Services.Heartbeats;
using Beter.Feed.TestingSandbox.Generator.Application.Services.TestScenarios.MessageHandlers;
using Beter.Feed.TestingSandbox.Generator.Domain.TestScenarios;
using Moq;
using System.Text.Json.Nodes;

namespace Beter.Feed.TestingSandbox.Generator.UnitTests.Application.Services.TestScenarios.MessageHandlers
{
    public class SteeringCommandMessageHandlerTests
    {
        private static readonly Fixture Fixture = new();

        private readonly Mock<IHeartbeatControlService> _heartbeatControlService;
        private readonly SteeringCommandMessageHandler _handler;

        public SteeringCommandMessageHandlerTests()
        {
            _heartbeatControlService = new Mock<IHeartbeatControlService>();
            _handler = new SteeringCommandMessageHandler(_heartbeatControlService.Object);
        }

        [Fact]
        public async Task Handle_SetsCommandToRun_WhenStartHeartbeatCommandReceived()
        {
            // Arrange
            var playbackId = Fixture.Create<Guid>();
            var additionInfo = Fixture.Create<AdditionalInfo>();
            var message = new TestScenarioMessage
            {
                MessageType = MessageTypes.SteeringCommand,
                Value = JsonNode.Parse("{\"CommandType\": 1 }")
            };

            // Act
            await _handler.Handle(message, playbackId, additionInfo, CancellationToken.None);

            // Assert
            _heartbeatControlService.Verify(h => h.SetCommand(HeartbeatCommand.Run), Times.Once);
            _heartbeatControlService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_SetsCommandToStop_WhenStopHeartbeatCommandReceived()
        {
            // Arrange
            var playbackId = Fixture.Create<Guid>();
            var additionInfo = Fixture.Create<AdditionalInfo>();
            var message = new TestScenarioMessage
            {
                MessageType = MessageTypes.SteeringCommand,
                Value = JsonNode.Parse("{\"CommandType\":2}")
            };

            // Act
            await _handler.Handle(message, playbackId, additionInfo, CancellationToken.None);

            // Assert
            _heartbeatControlService.Verify(h => h.SetCommand(HeartbeatCommand.Stop), Times.Once);
            _heartbeatControlService.VerifyNoOtherCalls();
        }

        [Fact]
        public void IsApplicable_ReturnsTrue_ForSteeringCommandMessageType()
        {
            // Act
            var isApplicable = _handler.IsApplicable(MessageTypes.SteeringCommand);

            // Assert
            Assert.True(isApplicable);
        }

        [Fact]
        public void IsApplicable_ReturnsFalse_ForNonSteeringCommandMessageType()
        {
            // Act
            var isApplicable = _handler.IsApplicable("SomeOtherMessageType");

            // Assert
            Assert.False(isApplicable);
        }
    }
}
