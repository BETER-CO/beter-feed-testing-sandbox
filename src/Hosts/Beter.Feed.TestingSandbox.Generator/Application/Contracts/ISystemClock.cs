namespace Beter.Feed.TestingSandbox.Generator.Application.Contracts;

public interface ISystemClock
{
    DateTimeOffset UtcNow { get; }
}