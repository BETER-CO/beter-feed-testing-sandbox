using AutoFixture;
using Beter.Feed.TestingSandbox.Common.Models;
using Beter.Feed.TestingSandbox.Generator.Infrastructure.Services.FeedConnections;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Net;

namespace Beter.Feed.TestingSandbox.Generator.UnitTests.Infrastructure.Services.FeedConnections
{
    public class FeedConnectionServiceTests
    {
        private const string MethodName = "SendAsync";
        private static readonly Fixture Fixture = new();

        private readonly Mock<HttpMessageHandler> _httpHandler;
        private readonly Mock<IFeedEmulatorUrlProvider> _urlProvider;

        private readonly FeedConnectionService _connectionService;

        public FeedConnectionServiceTests()
        {
            _httpHandler = new Mock<HttpMessageHandler>();
            _urlProvider = new Mock<IFeedEmulatorUrlProvider>();
            _connectionService = new FeedConnectionService(new HttpClient(_httpHandler.Object), _urlProvider.Object, new NullLogger<FeedConnectionService>());
        }

        [Fact]
        public async Task GetAsync_Returns_Deserialized_Connections()
        {
            // Arrange
            var url = Fixture.Create<Uri>();
            _urlProvider.Setup(x => x.GetConnections())
                .Returns(url);

            var expectedConnections = Fixture.CreateMany<FeedConnection>();
            var responseContent = new StringContent(JsonConvert.SerializeObject(expectedConnections));
            _httpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(MethodName, GetArgs(url, HttpMethod.Get))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = responseContent
                });

            // Act
            var connections = await _connectionService.GetAsync(CancellationToken.None);

            // Assert
            Assert.NotNull(connections);
            Assert.Equal(expectedConnections, connections);
        }

        [Fact]
        public async Task GetAsync_Throws_BadRequestException_When_Response_Not_OK()
        {
            // Arrange
            var url = Fixture.Create<Uri>();
            _urlProvider.Setup(x => x.GetConnections())
                .Returns(url);

            _httpHandler
               .Protected()
               .Setup<Task<HttpResponseMessage>>(MethodName, GetArgs(url, HttpMethod.Get))
               .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest });

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(
                () => _connectionService.GetAsync(CancellationToken.None));
        }

        [Fact]
        public async Task GetAsync_Throws_BadRequestException_When_Deserialization_Fails()
        {
            // Arrange
            var url = Fixture.Create<Uri>();
            _urlProvider.Setup(x => x.GetConnections())
                .Returns(url);

            var responseContent = new StringContent("Invalid JSON");
            _httpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(MethodName, GetArgs(url, HttpMethod.Get))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = responseContent });

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _connectionService.GetAsync(CancellationToken.None));
        }

        [Fact]
        public async Task DropConnectionAsync_Logs_Information_When_Connection_Dropped_Successfully()
        {
            // Arrange
            var connectionId = Fixture.Create<string>();
            var requestUri = Fixture.Create<Uri>();
            _urlProvider.Setup(x => x.DropConnection(connectionId))
                .Returns(requestUri);

            _httpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(MethodName, GetArgs(requestUri, HttpMethod.Delete))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK })
                .Verifiable();

            // Act
            await _connectionService.DropConnectionAsync(connectionId, CancellationToken.None);

            //Assert
            _httpHandler.Verify();
        }

        [Fact]
        public async Task DropConnectionAsync_Throws_BadRequestException_When_Response_Not_OK()
        {
            // Arrange
            var connectionId = Fixture.Create<string>();
            var requestUri = Fixture.Create<Uri>();
            _urlProvider.Setup(x => x.DropConnection(connectionId))
                .Returns(requestUri);

            _httpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(MethodName, GetArgs(requestUri, HttpMethod.Delete))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest });

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(
                () => _connectionService.DropConnectionAsync(connectionId, CancellationToken.None));
        }

        private static object[] GetArgs(Uri requestUri, HttpMethod method)
        {
            return new object[]
            {
                ItExpr.Is<HttpRequestMessage>(
                    r => r.RequestUri == requestUri && r.Method == method),
                ItExpr.IsAny<CancellationToken>()
            };

        }
    }
}
