namespace Beter.Feed.TestingSandbox.IntegrationTests.HttpClients.Abstract
{
    public interface IServiceReadinessChecker
    {
        Task WaitForServiceReadiness();
    }
}
