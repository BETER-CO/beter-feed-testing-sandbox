namespace Beter.TestingTools.Generator.Application.Contracts;

public interface ISystemClock
{
    DateTimeOffset UtcNow { get; }
}