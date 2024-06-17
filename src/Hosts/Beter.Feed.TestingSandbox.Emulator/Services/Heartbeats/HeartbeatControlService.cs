namespace Beter.Feed.TestingSandbox.Emulator.Services.Heartbeats
{
    public interface IHeartbeatControlService
    {
        HeartbeatCommand GetCurrentCommand();

        HeartbeatCommand SetCommand(int command);
    }

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
}
