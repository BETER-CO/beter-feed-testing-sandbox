using Beter.TestingTools.Generator.Application.Contracts;
using Beter.TestingTools.Generator.Application.Contracts.TestScenarios;
using Beter.TestingTools.Generator.Domain.TestScenarios;

namespace Beter.TestingTools.Generator.Application.Services.TestScenarios.MessageHandlers;

public abstract class BaseTestScenarioMessageHandler : ITestScenarioMessageHandler
{
    protected readonly IPublisher _publisher;

    public BaseTestScenarioMessageHandler(IPublisher publisher)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    }

    public async Task Handle(TestScenarioMessage message, string playbackId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        if (!IsApplicable(message.MessageType))
        {
            return;
        }

        await BeforePublish(message, playbackId, cancellationToken);
        await _publisher.PublishAsync(message, playbackId, cancellationToken);
        await AfterPublish(message, playbackId, cancellationToken);
    }

    public abstract bool IsApplicable(string messageType);
    public virtual Task BeforePublish(TestScenarioMessage message, string playbackId, CancellationToken cancellationToken) => Task.CompletedTask;
    public virtual Task AfterPublish(TestScenarioMessage message, string playbackId, CancellationToken cancellationToken) => Task.CompletedTask;
}
