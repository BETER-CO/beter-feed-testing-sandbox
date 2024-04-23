namespace Beter.TestingTools.Logging;

public class ConsoleSinkSettings : SinkSettings
{
    public const string OutputTemplate =
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fffzzz} [{Level:u3}] {ThreadId:d3} {Message:lj} - {SourceContext}{NewLine}{Exception}";

    public bool UseCompactJson { get; set; }

    public string Template { get; set; } = OutputTemplate;
}

