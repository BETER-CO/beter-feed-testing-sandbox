using Beter.Feed.TestingSandbox.Common.Models;
using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Generator.Application.Contracts.Heartbeats;
using Beter.Feed.TestingSandbox.Generator.Application.Contracts.TestScenarios;
using Beter.Feed.TestingSandbox.Generator.Application.Services.Heartbeats;
using Beter.Feed.TestingSandbox.Generator.Domain;
using Beter.Feed.TestingSandbox.Generator.Domain.TestScenarios;

namespace Beter.Feed.TestingSandbox.Generator.Application.Services.TestScenarios.MessageHandlers;

public class SteeringCommandMessageHandler : ITestScenarioMessageHandler
{
    private readonly IHeartbeatControlService _heartbeatControlService;

    public SteeringCommandMessageHandler(IHeartbeatControlService heartbeatControlService)
    {
        _heartbeatControlService = heartbeatControlService ?? throw new ArgumentNullException(nameof(heartbeatControlService));
    }

    public Task Handle(TestScenarioMessage message, Guid playbackId, AdditionalInfo additionalInfo, CancellationToken cancellationToken)
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
