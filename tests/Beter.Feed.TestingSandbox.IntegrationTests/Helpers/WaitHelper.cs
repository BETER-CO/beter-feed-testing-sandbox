namespace Beter.Feed.TestingSandbox.IntegrationTests.Helpers
{
    public static class WaitHelper
    {
        public static async Task WaitForCondition(Func<bool> condition, int delayMilliseconds = 3000, int maxRetries = 1000)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                if (condition())
                {
                    return;
                }

                await Task.Delay(delayMilliseconds);
            }

            throw new TimeoutException("Response was not processed within the specified time.");
        }
    }
}
