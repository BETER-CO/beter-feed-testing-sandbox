using AutoFixture;
using Beter.TestingTools.Emulator.Publishers;
using Beter.TestingTools.Emulator.Services;
using Beter.TestingTools.Emulator.SignalR.Hubs;
using Beter.TestingTools.Models.GlobalEvents;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Beter.TestingTools.Emulator.UnitTests.Publishers
{
    public class FeedMessagePublisherTests
    {
        private static readonly string GroupId = "testGroup";
        private static readonly Fixture Fixture = new();

        public class TestModel
        {
        }

        public class TestHub : BaseHub<TestModel, IFeedHub<TestModel>>
        {
            public TestHub(IMessagePublisher<TestModel> publisher, IConnectionManager connectionManager, ILogger<BaseHub<TestModel, IFeedHub<TestModel>>> logger)
                : base(publisher, connectionManager, logger)
            {
            }
        }

        private readonly Mock<IFeedHub<TestModel>> _hub;
        private readonly FeedMessagePublisher<TestHub, TestModel, IFeedHub<TestModel>> _publisher;

        public FeedMessagePublisherTests()
        {
            _hub = new Mock<IFeedHub<TestModel>>();
            var hubClients = new Mock<IHubCallerClients<IFeedHub<TestModel>>>();
            hubClients.Setup(m => m.Group(GroupId))
                .Returns(_hub.Object);

            var hubContext = new Mock<IHubContext<TestHub, IFeedHub<TestModel>>>();
            hubContext.Setup(x => x.Clients)
                .Returns(hubClients.Object);

            var logger = new NullLogger<FeedMessagePublisher<TestHub, TestModel, IFeedHub<TestModel>>>();
            _publisher = new FeedMessagePublisher<TestHub, TestModel, IFeedHub<TestModel>>(hubContext.Object, logger);
        }

        [Fact]
        public async Task SystemGroupPublish_SendsSystemEventsToGroup()
        {
            // Arrange
            var items = Fixture.CreateMany<GlobalMessageModel>();

            // Act
            await _publisher.SystemGroupPublish(GroupId, items, CancellationToken.None);

            // Assert
            _hub.Verify(hub => hub.OnSystemEvent(items, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task GroupPublish_SendsModelsToGroup()
        {
            // Arrange
            var models = Fixture.CreateMany<TestModel>();

            // Act
            await _publisher.GroupPublish(GroupId, models.ToArray(), CancellationToken.None);

            // Assert
            _hub.Verify(hub => hub.OnUpdate(models, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task SendGroupRemoveSubscriptionsAsync_SendsRemoveSubscriptionsToGroup()
        {
            // Arrange
            var ids = Fixture.CreateMany<string>().ToArray();

            // Act
            await _publisher.SendGroupRemoveSubscriptionsAsync(GroupId, ids, CancellationToken.None);

            // Assert
            _hub.Verify(hub => hub.OnSubscriptionsRemove(ids, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task SendGroupOnHeartbeatAsync_SendsHeartbeatToGroup()
        {
            // Act
            await _publisher.SendGroupOnHeartbeatAsync(GroupId, CancellationToken.None);

            // Assert
            _hub.Verify(hub => hub.OnHeartbeat(
                It.IsAny<long>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task GroupPublishEmptyArray_SendsEmptyArrayToGroup()
        {
            // Act
            await _publisher.GroupPublishEmptyArray(GroupId, CancellationToken.None);

            // Assert
            _hub.Verify(hub => hub.OnUpdate(Array.Empty<TestModel>(), CancellationToken.None), Times.Once);
        }
    }
}
