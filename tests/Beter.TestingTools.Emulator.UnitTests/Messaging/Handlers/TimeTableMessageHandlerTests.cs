using AutoFixture;
using Beter.TestingTools.Emulator.Messaging.Handlers;
using Beter.TestingTools.Emulator.Messaging;
using Beter.TestingTools.Emulator.Publishers;
using Beter.TestingTools.Models.Incidents;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beter.TestingTools.Models.TimeTableItems;
using Beter.TestingTools.Common.Constants;

namespace Beter.TestingTools.Emulator.UnitTests.Messaging.Handlers
{
    public class TimeTableMessageHandlerTests
    {
        private static readonly Fixture Fixture = new();

        [Fact]
        public void Constructor_NullPublisher_ThrowsArgumentNullException()
        {
            // Arrange
            IMessagePublisher<TimeTableItemModel> publisher = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(
                () => new TimeTableMessageHandler(publisher));
        }

        [Fact]
        public async Task HandleAsync_SendsMessageToPublisher()
        {
            // Arrange
            var publisher = new Mock<IMessagePublisher<TimeTableItemModel>>();
            var handler = new TimeTableMessageHandler(publisher.Object);

            var messages = Fixture.CreateMany<TimeTableItemModel>().ToArray();
            var context = Fixture.Create<ConsumeMessageContext>();

            // Act
            await handler.HandleAsync(messages, context, CancellationToken.None);

            // Assert
            publisher.Verify(p => p.GroupPublish(GroupNames.DefaultGroupName, messages, CancellationToken.None), Times.Once);
        }
    }
}
