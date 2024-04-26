using Beter.TestingTools.Generator.Application.Services.Heartbeats;

namespace Beter.TestingTools.Generator.Application.Contracts.Heartbeats;

public interface IHeartbeatControlService
{
    HeartbeatCommand GetCurrentCommand();

    HeartbeatCommand SetCommand(int command);
}