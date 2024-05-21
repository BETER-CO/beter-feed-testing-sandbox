using AutoFixture;
using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Common.Serialization;
using Beter.Feed.TestingSandbox.Generator.Domain.TestScenarios;
using Beter.Feed.TestingSandbox.Generator.Infrastructure.Options;
using Beter.Feed.TestingSandbox.Generator.Infrastructure.Services;
using Beter.Feed.TestingSandbox.Generator.UnitTests.Fixtures;
using Beter.Feed.TestingSandbox.Models;
using Confluent.Kafka;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Beter.Feed.TestingSandbox.Generator.UnitTests.Infrastructure.Services
{
    public class PublisherTests
    {
        private static readonly Fixture Fixture = new();

        public PublisherTests()
        {
            Fixture.Customizations.Add(new JsonNodeBuilder());
        }

        [Fact]
        public async Task PublishAsync_Successfully_Publishes_Heartbeat_Message()
        {
            // Arrange
            var topic = Fixture.Create<string>();
            var model = Fixture.Create<HeartbeatModel>();
            var expectedMessage = new Message<string, string>()
            {
                Headers = new Headers
                {
                    { HeaderNames.MessageType, Encoding.UTF8.GetBytes(MessageTypes.Heartbeat) },
                    { HeaderNames.PlaybackId, Encoding.UTF8.GetBytes("heartbeat-playback") }
                },
                Value = JsonHubSerializer.Serialize(model)
            };

            var producer = SetupProducer(topic, expectedMessage);
            var publisher = CreatePublisher(topic, producer.Object);

            // Act
            await publisher.PublishAsync(model, CancellationToken.None);

            // Assert
            producer.Verify(
                p => p.ProduceAsync(
                    topic,
                    It.Is<Message<string, string>>(actual => IsEqualsMessage(expectedMessage, actual)), CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task PublishEmptyAsync_Successfully_Publishes_Empty_Message()
        {
            // Arrange
            var topic = Fixture.Create<string>();
            var messageType = Fixture.Create<string>();
            var channel = Fixture.Create<string>();
            var playbackId = Fixture.Create<Guid>();
            var expectedMessage = new Message<string, string>()
            {
                Headers = new Headers
                {
                    { HeaderNames.MessageType, Encoding.UTF8.GetBytes(messageType) },
                    { HeaderNames.MessageChannel, Encoding.UTF8.GetBytes(channel) },
                    { HeaderNames.PlaybackId, Encoding.UTF8.GetBytes(playbackId.ToString()) }
                },
                Value = JsonSerializer.Serialize(Array.Empty<string>())
            };

            var producer = SetupProducer(topic, expectedMessage);
            var publisher = CreatePublisher(topic, producer.Object);

            // Act
            await publisher.PublishEmptyAsync(messageType, channel, playbackId, CancellationToken.None);

            // Assert
            producer.Verify(
                p => p.ProduceAsync(
                    topic,
                    It.Is<Message<string, string>>(actual => IsEqualsMessage(expectedMessage, actual)), CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task PublishAsync_Successfully_Publishes_TestScenarioMessage()
        {
            // Arrange
            var topic = Fixture.Create<string>();
            var model = Fixture.Create<TestScenarioMessage>();
            var playbackId = Fixture.Create<Guid>();
            var expectedMessage = new Message<string, string>()
            {
                Headers = new Headers
                {
                    { HeaderNames.MessageType, Encoding.UTF8.GetBytes(model.MessageType) },
                    { HeaderNames.MessageChannel, Encoding.UTF8.GetBytes(model.Channel) },
                    { HeaderNames.PlaybackId, Encoding.UTF8.GetBytes(playbackId.ToString()) }
                },
                Value = model.Value.ToJsonString()
            };

            var producer = SetupProducer(topic, expectedMessage);
            var publisher = CreatePublisher(topic, producer.Object);

            // Act
            await publisher.PublishAsync(model, playbackId, CancellationToken.None);

            // Assert
            producer.Verify(
                p => p.ProduceAsync(
                    topic,
                    It.Is<Message<string, string>>(actual => IsEqualsMessage(expectedMessage, actual)), CancellationToken.None),
                Times.Once);

        }

        [Fact]
        public void ValidateOptions_ThrowsException_WhenSslKeyLocationNotSpecified()
        {
            // Arrange
            var optionsMock = new Mock<IOptions<PublishOptions>>();
            var publishOptions = new PublishOptions
            {
                BootstrapServers = "localhost:9092",
                SecurityProtocol = SecurityProtocol.Ssl,
                SslCertificateLocation = "certificate.pem"
            };
            optionsMock.Setup(x => x.Value).Returns(publishOptions);

            // Act & Assert
            Assert.Throws<ArgumentException>(
                () => new Publisher(optionsMock.Object, new NullLogger<Publisher>()));
        }

        [Fact]
        public void ValidateOptions_ThrowsException_WhenSslCertificateLocationNotSpecified()
        {
            // Arrange
            var optionsMock = new Mock<IOptions<PublishOptions>>();
            var publishOptions = new PublishOptions
            {
                BootstrapServers = "localhost:9092",
                SecurityProtocol = SecurityProtocol.Ssl,
                SslKeyLocation = "key.pem"
            };
            optionsMock.Setup(x => x.Value).Returns(publishOptions);

            // Act & Assert
            Assert.Throws<ArgumentException>(
                () => new Publisher(optionsMock.Object, new NullLogger<Publisher>()));
        }

        [Fact]
        public void ValidateOptions_ThrowsException_WhenBootstrapServersNotSpecified()
        {
            // Arrange
            var optionsMock = new Mock<IOptions<PublishOptions>>();
            var publishOptions = new PublishOptions
            {
                SecurityProtocol = SecurityProtocol.Plaintext
            };
            optionsMock.Setup(x => x.Value).Returns(publishOptions);

            // Act & Assert
            Assert.Throws<ArgumentException>(
                () => new Publisher(optionsMock.Object, new NullLogger<Publisher>()));
        }

        [Fact]
        public void ValidateOptions_DoesNotThrowException_WhenValidOptionsProvided()
        {
            // Arrange
            var optionsMock = new Mock<IOptions<PublishOptions>>();
            var publishOptions = new PublishOptions
            {
                BootstrapServers = "localhost:9092",
                SecurityProtocol = SecurityProtocol.Plaintext
            };
            optionsMock.Setup(x => x.Value).Returns(publishOptions);

            //Act
            var publisher = new Publisher(optionsMock.Object, new NullLogger<Publisher>());

            //Assert
            Assert.True(true);
        }

        private Mock<IProducer<string, string>> SetupProducer(string topic, Message<string, string> message)
        {
            var producer = new Mock<IProducer<string, string>>();
            producer.Setup(p => p.ProduceAsync(topic, message, It.IsAny<CancellationToken>()))
                .Verifiable();

            return producer;
        }

        private static Publisher CreatePublisher(string topic, IProducer<string, string> producer)
        {
            var options = new Mock<IOptions<PublishOptions>>();
            options.SetupGet(opt => opt.Value)
                .Returns(new PublishOptions { BootstrapServers = "localhost:9092", Topic = topic });

            var publisher = new Publisher(options.Object, new NullLogger<Publisher>());
            publisher.GetType().GetField("_producer", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(publisher, producer);

            return publisher;
        }

        private static bool IsEqualsMessage(Message<string, string> expected, Message<string, string> actual)
        {
            var isEquals = expected.Value.SequenceEqual(actual.Value)
                && expected.Headers.Count == actual.Headers.Count;

            for (var i = 0; i < expected.Headers.Count(); i++)
            {
                isEquals = isEquals && expected.Headers[i].GetValueBytes().SequenceEqual(actual.Headers[i].GetValueBytes());
            }

            return isEquals;
        }
    }
}
