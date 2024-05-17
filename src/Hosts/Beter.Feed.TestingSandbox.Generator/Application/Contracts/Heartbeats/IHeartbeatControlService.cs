using Beter.Feed.TestingSandbox.Generator.Application.Services.Heartbeats;

namespace Beter.Feed.TestingSandbox.Generator.Application.Contracts.Heartbeats;

public interface IHeartbeatControlService
{
    HeartbeatCommand GetCurrentCommand();

    HeartbeatCommand SetCommand(int command);
}