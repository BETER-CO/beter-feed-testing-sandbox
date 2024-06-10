using Beter.Feed.TestingSandbox.Emulator.Messaging.Handlers.Abstract;
using Beter.Feed.TestingSandbox.Emulator.Services.Heartbeats;
using Beter.Feed.TestingSandbox.Models;

namespace Beter.Feed.TestingSandbox.Emulator.Messaging.Handlers
{
    public class SteeringCommandMessageHandler : MessageHandlerBase<SteeringCommandModel>
    {
        private readonly IHeartbeatControlService _heartbeatControlService;

        public SteeringCommandMessageHandler(IHeartbeatControlService heartbeatControlService)
        {
            _heartbeatControlService = heartbeatControlService ?? throw new ArgumentNullException(nameof(heartbeatControlService));
        }

        public override Task HandleAsync(SteeringCommandModel message, ConsumeMessageContext context, CancellationToken cancellationToken = default)
        {
            switch (message.CommandType)
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
    }
}
