using Beter.Feed.TestingSandbox.Common.Models;
using Beter.Feed.TestingSandbox.Generator.Domain.TestScenarios;

namespace Beter.Feed.TestingSandbox.Generator.Application.Contracts.TestScenarios;

public interface ITestScenarioMessageHandler
{
    bool IsApplicable(string messageType);
    Task Handle(TestScenarioMessage message, Guid playbackId, AdditionalInfo additionalInfo, CancellationToken cancellationToken);
}
