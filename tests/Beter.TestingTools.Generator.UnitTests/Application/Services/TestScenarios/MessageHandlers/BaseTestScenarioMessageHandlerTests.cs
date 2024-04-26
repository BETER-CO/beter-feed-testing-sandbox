using AutoFixture;
using Beter.TestingTools.Generator.Application.Contracts;
using Beter.TestingTools.Generator.Application.Services.TestScenarios.MessageHandlers;
using Beter.TestingTools.Generator.Domain.TestScenarios;
using Beter.TestingTools.Generator.UnitTests.Common;
using Moq;

namespace Beter.TestingTools.Generator.UnitTests.Application.Services.TestScenarios.MessageHandlers
{
    public class BaseTestScenarioMessageHandlerTests
    {
        private static readonly Fixture Fixture = new();

        private readonly Mock<IPublisher> _publisher;
        private readonly TestHandler _handler;

        public BaseTestScenarioMessageHandlerTests()
        {
            _publisher = new Mock<IPublisher>();
            _handler = new TestHandler(_publisher.Object);
        }

        [Fact]
        public void Ctor_should_has_null_guards()
        {
            AssertInjection.OfConstructor(typeof(TestHandler)).HasNullGuard();
        }

        [Fact]
        public async Task Handle_WithApplicableMessageType_CallsPublishMethods()
        {
            // Arrange
            var playbackId = Fixture.Create<string>();
            var message = new TestScenarioMessage { MessageType = "ApplicableMessageType" };

            // Act
            await _handler.Handle(message, playbackId, CancellationToken.None);

            // Assert
            _publisher.Verify(p => p.PublishAsync(message, playbackId, CancellationToken.None), Times.Once);
            Assert.True(_handler.BeforePublishCalled);
            Assert.True(_handler.AfterPublishCalled);
        }

        [Fact]
        public async Task Handle_WithNonApplicableMessageType_DoesNotCallPublishMethods()
        {
            // Arrange
            var playbackId = Fixture.Create<string>();
            var message = new TestScenarioMessage { MessageType = "NonApplicableMessageType" };

            // Act
            await _handler.Handle(message, playbackId, CancellationToken.None);

            // Assert
            _publisher.Verify(p => p.PublishAsync(message, playbackId, CancellationToken.None), Times.Never);
            Assert.False(_handler.BeforePublishCalled);
            Assert.False(_handler.AfterPublishCalled);
        }

        private class TestHandler : BaseTestScenarioMessageHandler
        {
            public bool BeforePublishCalled { get; private set; }
            public bool AfterPublishCalled { get; private set; }

            public TestHandler(IPublisher publisher) : base(publisher)
            {
            }

            public override bool IsApplicable(string messageType)
            {
                return messageType == "ApplicableMessageType";
            }

            public override async Task BeforePublish(TestScenarioMessage message, string playbackId, CancellationToken cancellationToken)
            {
                BeforePublishCalled = true;
                await base.BeforePublish(message, playbackId, cancellationToken);
            }

            public override async Task AfterPublish(TestScenarioMessage message, string playbackId, CancellationToken cancellationToken)
            {
                AfterPublishCalled = true;
                await base.AfterPublish(message, playbackId, cancellationToken);
            }
        }
    }
}
