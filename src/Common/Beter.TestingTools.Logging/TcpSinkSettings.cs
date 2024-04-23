namespace Beter.TestingTools.Logging;

public class TcpSinkSettings : SinkSettings
{
    public string? Address { get; set; }

    public int? Port { get; set; }

    public TcpSinkSettings()
    {
        Disabled = true;
    }
}

