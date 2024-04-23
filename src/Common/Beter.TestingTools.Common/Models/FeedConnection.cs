namespace Beter.TestingTools.Common.Models;

public sealed record FeedConnection
{
    public required string Id { get; set; }
    public required string FeedChannel { get; set; }
    public required DateTime Date { get; set; }
    public required string IpAddress { get; set; }

    public static FeedConnection Create(string id, string feedChannel, DateTime date, string ipAddress) => new()
    {
        Id = id,
        FeedChannel = feedChannel,
        Date = date,
        IpAddress = ipAddress
    };
}

