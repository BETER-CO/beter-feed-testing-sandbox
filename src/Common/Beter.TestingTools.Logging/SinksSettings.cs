namespace Beter.TestingTools.Logging;

public class SinksSettings
{
    public ConsoleSinkSettings? Console { get; set; }

    public FileSinkSettings? File { get; set; }

    public HttpSinkSettings? Http { get; set; }

    public TcpSinkSettings? Tcp { get; set; } = new();
}

