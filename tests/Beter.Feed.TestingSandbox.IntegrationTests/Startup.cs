using Beter.Feed.TestingSandbox.Emulator.Messaging;
using Beter.Feed.TestingSandbox.Generator.Application.Contracts.TestScenarios;
using Beter.Feed.TestingSandbox.Generator.Application.Services.TestScenarios;
using Beter.Feed.TestingSandbox.IntegrationTests.HttpClients;
using Beter.Feed.TestingSandbox.IntegrationTests.HttpClients.Abstract;
using Beter.Feed.TestingSandbox.IntegrationTests.Infrastructure;
using Beter.Feed.TestingSandbox.IntegrationTests.Options;
using Beter.Feed.TestingSandbox.IntegrationTests.Services;
using Microsoft.Extensions.DependencyInjection;
using ConsumerOptions = Beter.Feed.TestingSandbox.IntegrationTests.Options.ConsumerOptions;

namespace Beter.Feed.TestingSandbox.IntegrationTests
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IGeneratorServiceUrlProvider, GeneratorServiceUrlProvider>();
            services.AddSingleton<IGeneratorServiceHttpClient, GeneratorServiceHttpClient>();

            services.AddSingleton<ITestScenarioFactory, TestScenarioFactory>();
            services.AddSingleton<IFeedDataService, FeedDataService>();
            services.AddSingleton<IConsumeMessageConverter, ConsumeMessageConverter>();

            services.Configure<HttpClientsOptions>(TestConfiguration.Get.GetSection(HttpClientsOptions.Section));
            services.Configure<ConsumerOptions>(TestConfiguration.Get.GetSection(ConsumerOptions.Section));
        }
    }
}
