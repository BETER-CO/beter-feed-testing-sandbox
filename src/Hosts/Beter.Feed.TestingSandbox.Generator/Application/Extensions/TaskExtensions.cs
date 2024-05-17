namespace Beter.Feed.TestingSandbox.Generator.Application.Extensions;

public static class TaskExtensions
{
    public async static Task ExecuteWithTimeout(this Task task, TimeSpan timeout, Action onTimeout)
    {
        ArgumentNullException.ThrowIfNull(task);

        using var cancellationTokenSource = new CancellationTokenSource();
        var timeoutTask = Task.Delay(timeout, cancellationTokenSource.Token);

        var completedTask = await Task.WhenAny(task, timeoutTask);
        if (completedTask == timeoutTask)
        {
            onTimeout();
        }
        else
        {
            cancellationTokenSource.Cancel();
        }
    }
}
