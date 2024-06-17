using AutoFixture;
using Beter.Feed.TestingSandbox.Emulator.Messaging;
using Beter.Feed.TestingSandbox.Emulator.Messaging.Handlers;
using Beter.Feed.TestingSandbox.Emulator.Services.Heartbeats;
using Beter.Feed.TestingSandbox.Models;
using Moq;

namespace Beter.Feed.TestingSandbox.Emulator.UnitTests.Messaging.Handlers
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
        public async Task HandleAsync_SetsCommandToRun_WhenStartHeartbeatCommandReceived()
        {
            // Arrange
            var context = Fixture.Create<ConsumeMessageContext>();
            var model = Fixture.Build<SteeringCommandModel>()
                .With(x => x.CommandType, SteeringCommandType.StartHeartbeat)
                .Create();

            // Act
            await _handler.HandleAsync(model, context, CancellationToken.None);

            // Assert
            _heartbeatControlService.Verify(h => h.SetCommand(HeartbeatCommand.Run), Times.Once);
            _heartbeatControlService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_SetsCommandToStop_WhenStopHeartbeatCommandReceived()
        {
            // Arrange
            var context = Fixture.Create<ConsumeMessageContext>();
            var model = Fixture.Build<SteeringCommandModel>()
                .With(x => x.CommandType, SteeringCommandType.StopHeartbeat)
                .Create();

            // Act
            await _handler.HandleAsync(model, context, CancellationToken.None);

            // Assert
            _heartbeatControlService.Verify(h => h.SetCommand(HeartbeatCommand.Stop), Times.Once);
            _heartbeatControlService.VerifyNoOtherCalls();
        }
    }
}
