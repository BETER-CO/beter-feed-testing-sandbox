using AutoFixture;
using Beter.TestingTools.Common.Constants;
using Beter.TestingTools.Emulator.Messaging;
using Beter.TestingTools.Models.Scoreboards;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text;
using System.Text.Json;

namespace Beter.TestingTools.Emulator.UnitTests.Messaging
{
    public class ConsumeMessageConverterTests
    {
        private static readonly Fixture Fixture = new();

        private readonly ConsumeMessageConverter _converter;

        public ConsumeMessageConverterTests()
        {
            var logger = new NullLogger<ConsumeMessageConverter>();
            _converter = new ConsumeMessageConverter(logger);
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            // Arrange
            ILogger<ConsumeMessageConverter> logger = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(
                () => new ConsumeMessageConverter(logger));
        }

        [Fact]
        public void CanProcess_MessageWithoutTypeHeader_ReturnsFalse()
        {
            // Arrange
            var consumeResult = new ConsumeResult<byte[], byte[]>
            {
                Message = new Message<byte[], byte[]>
                {
                    Headers = new Headers()
                }
            };

            // Act
            var result = _converter.CanProcess(consumeResult);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CanProcess_MessageWithTypeHeaderButUnknownType_ReturnsFalse()
        {
            // Arrange
            var consumeResult = new ConsumeResult<byte[], byte[]>
            {
                Message = new Message<byte[], byte[]>
                {
                    Headers = new Headers
                    {
                        { HeaderNames.MessageType, Encoding.UTF8.GetBytes(Fixture.Create<string>()) }
                    }
                }
            };

            // Act
            var result = _converter.CanProcess(consumeResult);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CanProcess_MessageWithTypeHeaderAndKnownType_ReturnsTrue()
        {
            // Arrange
            var consumeResult = new ConsumeResult<byte[], byte[]>
            {
                Message = new Message<byte[], byte[]>
                {
                    Headers = new Headers
                    {
                        { HeaderNames.MessageType, Encoding.UTF8.GetBytes(MessageTypes.Incident) }
                    }
                }
            };

            // Act
            var result = _converter.CanProcess(consumeResult);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ConvertToMessageContextFromConsumeResult_WithMessageTypeAndMessageValue_ReturnsMessageContext()
        {
            // Arrange
            var messageValue = JsonSerializer.Serialize(Fixture.CreateMany<ScoreBoardModel>());
            var consumeResult = new ConsumeResult<byte[], byte[]>
            {
                Message = new Message<byte[], byte[]>
                {
                    Headers = new Headers
                    {
                        { HeaderNames.MessageType, Encoding.UTF8.GetBytes(MessageTypes.Scoreboard) }
                    },
                    Value = Encoding.UTF8.GetBytes(messageValue)
                }
            };

            // Act
            var result = _converter.ConvertToMessageContextFromConsumeResult(consumeResult);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(typeof(ScoreBoardModel[]), result.MessageType);
            Assert.Equal(messageValue, JsonSerializer.Serialize(result.MessageObject));
        }
    }
}
