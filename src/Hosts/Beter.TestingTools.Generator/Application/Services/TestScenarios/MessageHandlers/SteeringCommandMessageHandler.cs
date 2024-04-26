using Beter.TestingTools.Common.Constants;
using Beter.TestingTools.Generator.Application.Contracts.TestScenarios;
using Beter.TestingTools.Generator.Application.Services.Heartbeats;
using Beter.TestingTools.Generator.Domain;
using Beter.TestingTools.Generator.Domain.TestScenarios;
using Beter.TestingTools.Generator.Application.Contracts.Heartbeats;

namespace Beter.TestingTools.Generator.Application.Services.TestScenarios.MessageHandlers;

public class SteeringCommandMessageHandler : ITestScenarioMessageHandler
{
    private readonly IHeartbeatControlService _heartbeatControlService;

    public SteeringCommandMessageHandler(IHeartbeatControlService heartbeatControlService)
    {
        _heartbeatControlService = heartbeatControlService ?? throw new ArgumentNullException(nameof(heartbeatControlService));
    }

    public Task Handle(TestScenarioMessage message, string playbackId, CancellationToken cancellationToken)
    {
        var command = message.GetValue<SteeringCommand>();

        switch (command.CommandType)
        {
            case SteeringCommandType.StartHeartbeat:
                _heartbeatControlService.SetCommand(HeartbeatCommand.Run);
                break;
            case SteeringCommandType.StopHeartbeat:
                _heartbeatControlService.SetCommand(HeartbeatCommand.Stop);
                break;
            default:
                break;
        }

        return Task.CompletedTask;
    }

    public bool IsApplicable(string messageType) => messageType == MessageTypes.SteeringCommand;
}
