using Beter.Feed.TestingSandbox.Generator.Application.Common;

namespace Beter.Feed.TestingSandbox.Generator.Application.Services.Heartbeats;

public class HeartbeatCommand(int id, string name) : EnumerationItem(id, name)
{
    public readonly static HeartbeatCommand Run = new(1, nameof(Run));
    public readonly static HeartbeatCommand Stop = new(2, nameof(Stop));

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