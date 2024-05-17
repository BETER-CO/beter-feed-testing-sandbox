using Beter.Feed.TestingSandbox.Common.Models;
using Beter.Feed.TestingSandbox.Generator.Application.Contracts;
using Beter.Feed.TestingSandbox.Generator.Application.Contracts.TestScenarios;
using Beter.Feed.TestingSandbox.Generator.Domain.TestScenarios;

namespace Beter.Feed.TestingSandbox.Generator.Application.Services.TestScenarios.MessageHandlers;

public abstract class BaseTestScenarioMessageHandler : ITestScenarioMessageHandler
{
    protected readonly IPublisher _publisher;

    public BaseTestScenarioMessageHandler(IPublisher publisher)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    }

    public async Task Handle(TestScenarioMessage message, string playbackId, AdditionalInfo additionalInfo, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        if (!IsApplicable(message.MessageType))
        {
            return;
        }

        await BeforePublish(message, playbackId, additionalInfo, cancellationToken);
        await _publisher.PublishAsync(message, playbackId, cancellationToken);
        await AfterPublish(message, playbackId, additionalInfo, cancellationToken);
    }

    public abstract bool IsApplicable(string messageType);
    public virtual Task BeforePublish(TestScenarioMessage message, string playbackId, AdditionalInfo additionalInfo, CancellationToken cancellationToken) => Task.CompletedTask;
    public virtual Task AfterPublish(TestScenarioMessage message, string playbackId, AdditionalInfo additionalInfo, CancellationToken cancellationToken) => Task.CompletedTask;
}
