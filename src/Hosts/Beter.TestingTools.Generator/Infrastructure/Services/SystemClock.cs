using Beter.TestingTools.Generator.Application.Contracts;

namespace Beter.TestingTools.Generator.Infrastructure.Services;

public class SystemClock : ISystemClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

