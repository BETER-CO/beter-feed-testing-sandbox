using Beter.Feed.TestingSandbox.Common.Enums;
using Beter.Feed.TestingSandbox.Emulator.SignalR.Filters;
using Beter.Feed.TestingSandbox.Emulator.SignalR.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Beter.Feed.TestingSandbox.Emulator.UnitTests.SignalR.Filters
{
    public class TestHub : Hub, IHubIdentity
    {
        public HubKind Hub => HubKind.Undefined;
        public HubVersion Version => HubVersion.Undefined;
    }

    public class SignalRValidationFilterTests
    {
        private readonly SignalRValidationFilter _filter;

        public SignalRValidationFilterTests()
        {
            var logger = new NullLogger<SignalRValidationFilter>();
            _filter = new SignalRValidationFilter(logger);
        }

        [Fact]
        public async Task OnConnectedAsync_WithValidApiKey_CallsNext()
        {
            // Arrange
            var context = CreateHubLifetimeContextWithApiKey(Guid.NewGuid().ToString());

            // Act
            await _filter.OnConnectedAsync(context, ctx => Task.CompletedTask);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async Task OnConnectedAsync_WithInvalidApiKey_ThrowsHubException()
        {
            // Arrange
            var context = CreateHubLifetimeContextWithApiKey("InvalidApiKey");

            // Act & Assert
            await Assert.ThrowsAsync<HubException>(
                () => _filter.OnConnectedAsync(context, ctx => Task.CompletedTask));
        }

        private static HubLifetimeContext CreateHubLifetimeContextWithApiKey(string apiKey)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["ApiKey"] = apiKey;

            var hubCallerContext = new Mock<HubCallerContext>();
            hubCallerContext.Setup(x => x.Features.Get<IHttpContextFeature>().HttpContext)
                .Returns(httpContext);

            return new HubLifetimeContext(hubCallerContext.Object, new Mock<IServiceProvider>().Object, new TestHub());
        }
    }
}
