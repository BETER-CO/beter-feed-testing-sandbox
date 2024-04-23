namespace Beter.TestingTools.Emulator.SignalR.Settings;

public class SignalRSettings
{
    public const string SECTION_NAME = "SignalR";
    public string FeedTimeTableEndpoint { get; set; }
    public string FeedTradingEndpoint { get; set; }
    public string FeedScoreboardEndpoint { get; set; }
    public string FeedIncidentEndpoint { get; set; }
}
