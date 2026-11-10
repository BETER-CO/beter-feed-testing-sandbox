using Beter.Feed.TestingSandbox.Consumer.Clients;
using Beter.Feed.TestingSandbox.Consumer.Consumers;
using Beter.Feed.TestingSandbox.Consumer.Options;
using Beter.Feed.TestingSandbox.Consumer.Sinks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Beter.Feed.TestingSandbox.Consumer.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddHostedServices(this IServiceCollection services)
    {
        services.AddHostedService<TradingConsumer>();
        services.AddHostedService<TimeTableConsumer>();
        services.AddHostedService<ScoreboardConsumer>();
        services.AddHostedService<IncidentConsumer>();
    }

    /// <summary>
    /// Registers the <see cref="FeedServiceOptions"/> from the <c>FeedService</c> section of
    /// <paramref name="configuration"/>, the four hub clients and the default
    /// <see cref="LoggingFeedMessageSink"/>. A sink registered before this call is kept.
    /// Hosted services are not registered; see <see cref="AddHostedServices"/>.
    /// </summary>
    public static IServiceCollection AddFeedServiceClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging();
        services.Configure<FeedServiceOptions>(configuration.GetSection(FeedServiceOptions.SectionName));
        services.TryAddSingleton<IFeedMessageSink, LoggingFeedMessageSink>();

        services.AddSingleton<TradingFeedServiceClient>();
        services.AddSingleton<TimeTableFeedServiceClient>();
        services.AddSingleton<ScoreboardFeedServiceClient>();
        services.AddSingleton<IncidentFeedServiceClient>();

        return services;
    }
}
