using Beter.Feed.TestingSandbox.IntegrationTests.HttpClients;
using Beter.Feed.TestingSandbox.IntegrationTests.HttpClients.Abstract;
using Beter.Feed.TestingSandbox.IntegrationTests.Infrastructure;
using Beter.Feed.TestingSandbox.IntegrationTests.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Beter.Feed.TestingSandbox.IntegrationTests
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IGeneratorServiceUrlProvider, GeneratorServiceUrlProvider>();
            services.AddSingleton<IGeneratorServiceHttpClient, GeneratorServiceHttpClient>();

            services.AddSingleton<IConsumerServiceUrlProvider, ConsumerServiceUrlProvider>();
            services.AddSingleton<IConsumerServiceHttpClient, ConsumerServiceHttpClient>();

            services.Configure<HttpClientsOptions>(TestConfiguration.Get.GetSection(HttpClientsOptions.Section));
        }
    }
}
