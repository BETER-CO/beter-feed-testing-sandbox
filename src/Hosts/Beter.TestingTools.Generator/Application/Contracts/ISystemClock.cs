namespace Beter.TestingTool.Generator.Application.Contracts;

public interface ISystemClock
{
    DateTimeOffset UtcNow { get; }
}