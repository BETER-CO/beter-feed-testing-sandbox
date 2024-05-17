namespace Beter.Feed.TestingSandbox.Generator.Application.Contracts.TestScenarios;

public interface ITestScenarioMessageHandlerResolver
{
    ITestScenarioMessageHandler Resolve(string messageType);
}

