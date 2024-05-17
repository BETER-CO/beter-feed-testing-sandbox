using AutoFixture;
using Beter.Feed.TestingSandbox.Generator.Infrastructure.Options;
using Beter.Feed.TestingSandbox.Generator.Infrastructure.Services.FeedConnections;
using Microsoft.Extensions.Options;

namespace Beter.Feed.TestingSandbox.Generator.UnitTests.Infrastructure.Services.FeedConnections
{
    public class FeedEmulatorUrlProviderTests
    {
        private readonly IFixture _fixture;
        private readonly FeedEmulatorUrlProvider _provider;

        public FeedEmulatorUrlProviderTests()
        {
            _fixture = new Fixture();
            var apiBaseUrl = "https://testing-tools.com";
            var options = new FeedEmulatorOptions { ApiBaseUrl = apiBaseUrl };

            _provider = new FeedEmulatorUrlProvider(Options.Create(options));
        }

        [Fact]
        public void DropConnection_Returns_Correct_Uri()
        {
            //Arrange
            var connectionId = "10";
            var expectedUri = new Uri("https://testing-tools.com/api/connections/10");

            // Act
            var uri = _provider.DropConnection(connectionId);

            // Assert
            Assert.Equal(expectedUri, uri);
        }

        [Fact]
        public void GetConnections_Returns_Correct_Uri()
        {
            //Arrange
            var expectedUri = new Uri("https://testing-tools.com/api/connections");

            // Act
            var uri = _provider.GetConnections();

            // Assert
            Assert.Equal(expectedUri, uri);
        }
    }
}
