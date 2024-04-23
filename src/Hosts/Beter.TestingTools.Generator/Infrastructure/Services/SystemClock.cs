using Beter.TestingTool.Generator.Application.Contracts;

namespace Beter.TestingTool.Generator.Infrastructure.Services;

public class SystemClock : ISystemClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

