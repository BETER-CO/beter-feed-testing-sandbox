using AutoFixture;
using Beter.Feed.TestingSandbox.Common.Models;
using Beter.Feed.TestingSandbox.Generator.Application.Contracts;
using Beter.Feed.TestingSandbox.Generator.Application.Services.TestScenarios.MessageHandlers;
using Beter.Feed.TestingSandbox.Generator.Domain.TestScenarios;
using Beter.Feed.TestingSandbox.Generator.UnitTests.Common;
using Moq;

namespace Beter.Feed.TestingSandbox.Generator.UnitTests.Application.Services.TestScenarios.MessageHandlers
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
            var additionInfo = Fixture.Create<AdditionalInfo>();

            // Act
            await _handler.Handle(message, playbackId, additionInfo, CancellationToken.None);

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
            var additionInfo = Fixture.Create<AdditionalInfo>();

            // Act
            await _handler.Handle(message, playbackId, additionInfo, CancellationToken.None);

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

            public override async Task BeforePublish(TestScenarioMessage message, string playbackId, AdditionalInfo additionInfo, CancellationToken cancellationToken)
            {
                BeforePublishCalled = true;
                await base.BeforePublish(message, playbackId, additionInfo, cancellationToken);
            }

            public override async Task AfterPublish(TestScenarioMessage message, string playbackId, AdditionalInfo additionInfo, CancellationToken cancellationToken)
            {
                AfterPublishCalled = true;
                await base.AfterPublish(message, playbackId, additionInfo, cancellationToken);
            }
        }
    }
}
