using Beter.Feed.TestingSandbox.Generator.Domain.TestScenarios;
using Beter.Feed.TestingSandbox.Models;

namespace Beter.Feed.TestingSandbox.Generator.Application.Contracts;

public interface IPublisher
{
    Task PublishAsync(TestScenarioMessage message, Guid playbackId, CancellationToken cancellationToken);
    Task PublishAsync(HeartbeatModel heartbeatNotification, CancellationToken cancellationToken);
    Task PublishEmptyAsync(string messageType, string channel, Guid playbackId, CancellationToken cancellationToken);
}