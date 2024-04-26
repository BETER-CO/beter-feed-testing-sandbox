using Beter.TestingTools.Generator.Domain.TestScenarios;

namespace Beter.TestingTools.Generator.Application.Contracts.TestScenarios;

public interface ITestScenarioMessageHandler
{
    bool IsApplicable(string messageType);
    Task Handle(TestScenarioMessage message, string playbackId, CancellationToken cancellationToken);
}
