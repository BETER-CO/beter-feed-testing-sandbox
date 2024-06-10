using Beter.Feed.TestingSandbox.Common;

namespace Beter.Feed.TestingSandbox.Emulator.Services.Heartbeats
{
    public class HeartbeatCommand : EnumerationItem
    {
        public readonly static HeartbeatCommand Run = new(1, nameof(Run));
        public readonly static HeartbeatCommand Stop = new(2, nameof(Stop));

        public HeartbeatCommand(int id, string name) : base(id, name)
        {
        }

        public static void EnsureThatIsSupported(int command)
        {
            if (!IsSupport(command))
            {
                throw new InvalidOperationException($"Heartbeat commnad '{command}' is not supported.");
            }
        }

        public static HeartbeatCommand Get(int command)
        {
            EnsureThatIsSupported(command);

            return GetAll().Single(c => c.Id == command);
        }
        public static bool IsSupport(int command) => GetAll().Any(c => c.Id == command);
        public static bool IsRunStatus(HeartbeatCommand command) => command.Equals(Run);
        public static IEnumerable<HeartbeatCommand> GetAll()
        {
            yield return Run;
            yield return Stop;
        }
    }
}
