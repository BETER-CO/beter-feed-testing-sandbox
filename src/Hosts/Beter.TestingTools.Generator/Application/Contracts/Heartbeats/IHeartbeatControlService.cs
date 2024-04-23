using Beter.TestingTool.Generator.Application.Services.Heartbeats;

namespace Beter.TestingTool.Generator.Application.Contracts.Heartbeats;

public interface IHeartbeatControlService
{
    HeartbeatCommand GetCurrentCommand();

    HeartbeatCommand SetCommand(int command);
}