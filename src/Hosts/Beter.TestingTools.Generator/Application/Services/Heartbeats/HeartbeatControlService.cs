using Beter.TestingTool.Generator.Application.Contracts.Heartbeats;

namespace Beter.TestingTool.Generator.Application.Services.Heartbeats;

public class HeartbeatControlService : IHeartbeatControlService
{
    private readonly object _lock = new();
    private HeartbeatCommand _currentState = HeartbeatCommand.Run;

    public HeartbeatCommand SetCommand(int command)
    {
        lock (_lock)
        {
            _currentState = HeartbeatCommand.Get(command);
        }

        return _currentState;
    }

    public HeartbeatCommand GetCurrentCommand()
    {
        lock (_lock)
        {
            return _currentState;
        }
    }
}