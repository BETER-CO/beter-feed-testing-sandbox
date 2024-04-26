using Beter.TestingTools.Generator.Application.Contracts.TestScenarios;

namespace Beter.TestingTools.Generator.Application.Services.TestScenarios.MessageHandlers;

public sealed class TestScenarioMessageHandlerResolver : ITestScenarioMessageHandlerResolver
{
    private readonly IEnumerable<ITestScenarioMessageHandler> _commandHandlers;

    public TestScenarioMessageHandlerResolver(IEnumerable<ITestScenarioMessageHandler> commandHandlers)
    {
        _commandHandlers = commandHandlers ?? throw new ArgumentNullException(nameof(commandHandlers));
    }

    public ITestScenarioMessageHandler Resolve(string messageType)
    {
        var commandHandler = _commandHandlers.FirstOrDefault(handler => handler.IsApplicable(messageType));
        if (commandHandler == null)
        {
            throw new InvalidOperationException($"Unsupported message type {messageType}.");
        }

        return commandHandler;
    }
}