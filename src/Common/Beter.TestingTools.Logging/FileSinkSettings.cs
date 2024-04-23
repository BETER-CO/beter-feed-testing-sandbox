namespace Beter.TestingTools.Logging;

public class FileSinkSettings : SinkSettings
{
    public const int FileSizeLimitBytes = 100 * 1024 * 1024;

    public const string OutputTemplate =
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fffzzz} [{Level:u3}] {ThreadId:d3} {Message:lj} - {SourceContext}{NewLine}{Exception}";

    public string Template { get; set; } = OutputTemplate;

    public int MaxFileSize { get; set; } = FileSizeLimitBytes;
}

