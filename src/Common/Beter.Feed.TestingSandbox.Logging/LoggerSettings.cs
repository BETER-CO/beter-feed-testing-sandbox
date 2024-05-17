using Serilog.Events;

namespace Beter.Feed.TestingSandbox.Logging;

public class LoggerSettings
{
    public LogEventLevel MinimumLogLevel { get; set; } = LogEventLevel.Debug;

    public bool EnableSelfLog { get; set; }

    public int AsyncQueueLength { get; set; } = 500;

    public SinksSettings Sinks { get; set; } = new();
}
