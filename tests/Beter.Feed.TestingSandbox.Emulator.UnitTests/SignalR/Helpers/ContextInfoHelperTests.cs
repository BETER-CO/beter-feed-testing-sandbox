using Beter.Feed.TestingSandbox.Emulator.SignalR.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Primitives;
using Moq;

namespace Beter.Feed.TestingSandbox.Emulator.UnitTests.SignalR.Helpers
{
    public class ContextInfoHelperTests
    {
        [Fact]
        public void GetKey_ReturnsGuid_WhenHeaderAndQueryContainValidKey()
        {
            // Arrange
            var apiKey = Guid.NewGuid().ToString();
            var httpContext = CreateHttpContextWithApiKey(apiKey);

            var hubCallerContext = new Mock<HubCallerContext>();
            hubCallerContext.Setup(x => x.Features.Get<IHttpContextFeature>().HttpContext)
                .Returns(httpContext);

            // Act
            var result = hubCallerContext.Object.GetKey();

            // Assert
            Assert.Equal(Guid.Parse(apiKey), result);
        }

        [Fact]
        public void GetKey_ReturnsEmptyGuid_WhenHeaderAndQueryDoNotContainValidKey()
        {
            // Arrange
            var httpContext = CreateHttpContextWithApiKey(null);
            var hubCallerContext = new Mock<HubCallerContext>();
            hubCallerContext.Setup(x => x.Features.Get<IHttpContextFeature>().HttpContext)
                .Returns(httpContext);

            // Act
            var result = hubCallerContext.Object.GetKey();

            // Assert
            Assert.Equal(Guid.Empty, result);
        }

        [Theory]
        [InlineData(ContextInfoHelper.CloudFlareHeaderIp)]
        [InlineData(ContextInfoHelper.ForwardedHeader)]
        [InlineData(ContextInfoHelper.RealIpHeader)]
        public void GetIp_ReturnsIpAddress_WhenHeaderContainsValidIpAddress(string headerName)
        {
            // Arrange
            var ipAddress = "192.168.1.1";
            var httpContext = CreateHttpContextWithIpAddress(headerName, ipAddress);
            var hubCallerContext = new Mock<HubCallerContext>();
            hubCallerContext.Setup(x => x.Features.Get<IHttpContextFeature>().HttpContext)
                .Returns(httpContext);

            // Act
            var result = hubCallerContext.Object.GetIp();

            // Assert
            Assert.Equal(ipAddress, result);
        }

        private HttpContext CreateHttpContextWithApiKey(string apiKey)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["ApiKey"] = apiKey;

            return httpContext;
        }

        private HttpContext CreateHttpContextWithIpAddress(string headerName, string ipAddress)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[headerName] = new StringValues(ipAddress);

            return httpContext;
        }
    }
}
