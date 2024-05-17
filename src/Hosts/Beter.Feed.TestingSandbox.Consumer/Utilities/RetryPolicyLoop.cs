using Microsoft.AspNetCore.SignalR.Client;

namespace Beter.Feed.TestingSandbox.Consumer.Utilities;

public class RetryPolicyLoop : IRetryPolicy
{
    private readonly double _reconnectionWaitSeconds;

    public RetryPolicyLoop(double reconnectionWaitSeconds)
    {
        _reconnectionWaitSeconds = reconnectionWaitSeconds;
    }

    public TimeSpan? NextRetryDelay(RetryContext retryContext)
    {
        return TimeSpan.FromSeconds(_reconnectionWaitSeconds);
    }
}
