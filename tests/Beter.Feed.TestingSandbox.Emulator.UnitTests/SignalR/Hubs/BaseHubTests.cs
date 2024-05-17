using AutoFixture;
using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Common.Models;
using Beter.Feed.TestingSandbox.Emulator.Publishers;
using Beter.Feed.TestingSandbox.Emulator.Services;
using Beter.Feed.TestingSandbox.Emulator.SignalR.Hubs;
using Castle.Core.Logging;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using static Beter.Feed.TestingSandbox.Emulator.UnitTests.Publishers.FeedMessagePublisherTests;

namespace Beter.Feed.TestingSandbox.Emulator.UnitTests.SignalR.Hubs
{
    public class BaseHubTests
    {
        private static readonly Fixture Fixture = new();

        private readonly string _connectionId;
        private readonly Mock<IConnectionManager> _connectionManager;
        private readonly Mock<IMessagePublisher<TestModel>> _publisher;
        private readonly BaseHub<TestModel, IFeedHub<TestModel>> _hub;

        public BaseHubTests()
        {
            var logger = new NullLogger<BaseHub<TestModel, IFeedHub<TestModel>>>();
            _connectionId = Fixture.Create<string>();
            _connectionManager = new Mock<IConnectionManager>();
            _publisher = new Mock<IMessagePublisher<TestModel>>();
            _hub = new BaseHub<TestModel, IFeedHub<TestModel>>(_publisher.Object, _connectionManager.Object, logger);

            var groupManager = new Mock<IGroupManager>();
            groupManager.Setup(x => x.AddToGroupAsync("connectionId", GroupNames.DefaultGroupName, default))
                .Returns(Task.CompletedTask);

            var httpContext = new DefaultHttpContext();
            var hubCallerContext = new Mock<HubCallerContext>();

            hubCallerContext.Setup(x => x.Features.Get<IHttpContextFeature>().HttpContext)
                .Returns(httpContext);
            hubCallerContext.Setup(x => x.ConnectionId)
                .Returns(_connectionId);
            hubCallerContext.Setup(x => x.Features.Get<IConnectionHeartbeatFeature>())
               .Returns(new Mock<IConnectionHeartbeatFeature>().Object);

            _hub.Context = hubCallerContext.Object;
            _hub.Groups = groupManager.Object;
        }

        [Fact]
        public async Task OnConnectedAsync_SetsUpConnection_AddsToDefaultGroup_PublishesEmptyArray()
        {
            // Act
            await _hub.OnConnectedAsync();

            // Assert
            _connectionManager.Verify(m => m.Set(It.Is<FeedConnection>(x => x.Id == _connectionId)), Times.Once);
            _connectionManager.Verify(m => m.Remove(_connectionId), Times.Never);
            _publisher.Verify(m => m.GroupPublishEmptyArray(GroupNames.DefaultGroupName, default), Times.Once);
        }

        [Fact]
        public async Task OnDisconnectedAsync_RemovesConnection()
        {
            // Act
            await _hub.OnDisconnectedAsync(null);

            // Assert
            _connectionManager.Verify(m => m.Remove(_connectionId), Times.Once);
        }
    }
}
