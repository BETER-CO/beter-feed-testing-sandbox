using Serilog.Events;

namespace Beter.TestingTools.Logging;

public class SinkSettings
{
    public LogEventLevel LogLevel { get; set; } = LogEventLevel.Warning;

    public bool Disabled { get; set; }
}
