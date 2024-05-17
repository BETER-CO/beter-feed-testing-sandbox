using Microsoft.Extensions.Configuration;

namespace Beter.Feed.TestingSandbox.IntegrationTests.Infrastructure
{
    static class TestConfiguration
    {
        private static IConfigurationRoot _root;
        public static IConfigurationRoot Get => _root ??= new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true)
            .AddEnvironmentVariables()
            .Build();
    }
}
