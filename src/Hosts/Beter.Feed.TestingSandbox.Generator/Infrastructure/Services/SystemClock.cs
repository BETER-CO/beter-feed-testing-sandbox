using Beter.Feed.TestingSandbox.Generator.Application.Contracts;

namespace Beter.Feed.TestingSandbox.Generator.Infrastructure.Services;

public class SystemClock : ISystemClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

