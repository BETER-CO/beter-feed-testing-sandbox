using Beter.TestingTools.Models;
using Beter.TestingTools.Generator.Domain.TestScenarios;

namespace Beter.TestingTools.Generator.Application.Contracts;

public interface IPublisher
{
    Task PublishAsync(TestScenarioMessage message, string playbackId, CancellationToken cancellationToken);
    Task PublishAsync(HeartbeatModel heartbeatNotification, CancellationToken cancellationToken);
    Task PublishEmptyAsync(string messageType, string channel, string playbackId, CancellationToken cancellationToken);
}