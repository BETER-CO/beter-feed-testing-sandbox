namespace Beter.Feed.TestingSandbox.IntegrationTests.HttpClients.Abstract
{
    public interface IConsumerServiceUrlProvider
    {
        Uri BaseUrl();
        Uri LoadTestScenario();
        Uri GetTemplate();
    }
}
