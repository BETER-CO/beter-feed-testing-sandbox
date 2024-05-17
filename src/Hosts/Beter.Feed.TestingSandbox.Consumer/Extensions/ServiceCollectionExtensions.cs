using Beter.Feed.TestingSandbox.Consumer.Clients;
using Beter.Feed.TestingSandbox.Consumer.Consumers;
using Beter.Feed.TestingSandbox.Consumer.Models;
using Beter.Feed.TestingSandbox.Consumer.Options;
using Beter.Feed.TestingSandbox.Consumer.Producers;
using Beter.Feed.TestingSandbox.Consumer.Services;
using Beter.Feed.TestingSandbox.Consumer.Services.Abstract;
using Beter.Feed.TestingSandbox.Models;
using Beter.Feed.TestingSandbox.Models.GlobalEvents;
using Beter.Feed.TestingSandbox.Models.Incidents;
using Beter.Feed.TestingSandbox.Models.Scoreboards;
using Beter.Feed.TestingSandbox.Models.TimeTableItems;
using Beter.Feed.TestingSandbox.Models.TradingInfos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.FeatureManagement;

namespace Beter.Feed.TestingSandbox.Consumer.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFeatureManagement(configuration.GetSection(FeatureManagementFlags.Section));

        services.AddSingleton<ITestScenarioTemplateService, TestScenarioTemplateService>();
        services.AddSingleton<ITestScenarioFactory, TestScenarioFactory>();
    }

    public static void AddHostedServices(this IServiceCollection services)
    {
        services.AddHostedService<TradingConsumer>();
        services.AddHostedService<TimeTableConsumer>();
        services.AddHostedService<ScoreboardConsumer>();
        services.AddHostedService<IncidentConsumer>();
    }

    public static void AddFeedServiceClients(this IServiceCollection services)
    {
        services.AddSingleton<TradingFeedServiceClient>();
        services.AddSingleton<TimeTableFeedServiceClient>();
        services.AddSingleton<ScoreboardFeedServiceClient>();
        services.AddSingleton<IncidentFeedServiceClient>();
    }

    public static IServiceCollection AddKafkaConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FeedServiceOptions>(configuration.GetSection(FeedServiceOptions.SectionName));
        services.Configure<PublishOptions>(configuration.GetSection(PublishOptions.SectionName));
        services.AddSingleton<IProducerFactory, ProducerFactory>();

        return services;
    }

    public static void AddFeedMessageProducers(this IServiceCollection services)
    {
        services.AddKafkaMessageProducer<FeedMessageModel<IEnumerable<TimeTableItemModel>>, FeedMessageConverter<IEnumerable<TimeTableItemModel>>>();
        services.AddKafkaMessageProducer<FeedMessageModel<IEnumerable<ScoreBoardModel>>, FeedMessageConverter<IEnumerable<ScoreBoardModel>>>();
        services.AddKafkaMessageProducer<FeedMessageModel<IEnumerable<TradingInfoModel>>, FeedMessageConverter<IEnumerable<TradingInfoModel>>>();
        services.AddKafkaMessageProducer<FeedMessageModel<IEnumerable<IncidentModel>>, FeedMessageConverter<IEnumerable<IncidentModel>>>();
        services.AddKafkaMessageProducer<FeedMessageModel<IEnumerable<GlobalMessageModel>>, FeedMessageConverter<IEnumerable<GlobalMessageModel>>>();
        services.AddKafkaMessageProducer<FeedMessageModel<SubscriptionsRemovedModel>, FeedMessageConverter<SubscriptionsRemovedModel>>();

        services.AddSingleton<IFeedMessageProducer<TimeTableItemModel>, FeedMessageProducer<TimeTableItemModel>>();
        services.AddSingleton<IFeedMessageProducer<ScoreBoardModel>, FeedMessageProducer<ScoreBoardModel>>();
        services.AddSingleton<IFeedMessageProducer<TradingInfoModel>, FeedMessageProducer<TradingInfoModel>>();
        services.AddSingleton<IFeedMessageProducer<IncidentModel>, FeedMessageProducer<IncidentModel>>();
    }

    public static IServiceCollection AddKafkaMessageProducer<T, TMessageConverter>(this IServiceCollection serviceCollection)
       where TMessageConverter : class, IProducerMessageConverter<T>
    {
        serviceCollection.TryAddSingleton<IProducerMessageConverter<T>, TMessageConverter>();
        serviceCollection.TryAddSingleton<IProducerTransport<T>, ProducerTransport<T>>();
        return serviceCollection;
    }
}
