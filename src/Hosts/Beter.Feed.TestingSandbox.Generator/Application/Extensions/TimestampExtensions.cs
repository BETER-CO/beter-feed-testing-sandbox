namespace Beter.Feed.TestingSandbox.Generator.Application.Extensions;

public static class TimestampExtensions
{
    public static DateTime ToUtcDateTime(this long timestamp) => DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime;
}
