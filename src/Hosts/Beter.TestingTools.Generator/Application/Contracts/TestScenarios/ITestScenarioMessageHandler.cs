using Beter.TestingTool.Generator.Domain.TestScenarios;

namespace Beter.TestingTool.Generator.Application.Contracts.TestScenarios;

public interface ITestScenarioMessageHandler
{
    bool IsApplicable(string messageType);
    Task Handle(TestScenarioMessage message, string playbackId, CancellationToken cancellationToken);
}
