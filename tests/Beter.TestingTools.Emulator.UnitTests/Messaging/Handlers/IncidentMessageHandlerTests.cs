using AutoFixture;
using Beter.TestingTools.Common.Constants;
using Beter.TestingTools.Emulator.Messaging;
using Beter.TestingTools.Emulator.Messaging.Handlers;
using Beter.TestingTools.Emulator.Publishers;
using Beter.TestingTools.Models.Incidents;
using Moq;

namespace Beter.TestingTools.Emulator.UnitTests.Messaging.Handlers
{
    public class IncidentMessageHandlerTests
    {
        private static readonly Fixture Fixture = new();

        [Fact]
        public void Constructor_NullPublisher_ThrowsArgumentNullException()
        {
            // Arrange
            IMessagePublisher<IncidentModel> publisher = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(
                () => new IncidentMessageHandler(publisher));
        }

        [Fact]
        public async Task HandleAsync_SendsMessageToPublisher()
        {
            // Arrange
            var publisher = new Mock<IMessagePublisher<IncidentModel>>();
            var handler = new IncidentMessageHandler(publisher.Object);

            var messages = Fixture.CreateMany<IncidentModel>().ToArray();
            var context = Fixture.Create<ConsumeMessageContext>();

            // Act
            await handler.HandleAsync(messages, context, CancellationToken.None);

            // Assert
            publisher.Verify(p => p.GroupPublish(GroupNames.DefaultGroupName, messages, CancellationToken.None), Times.Once);
        }
    }
}
