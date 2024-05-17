using Beter.Feed.TestingSandbox.Emulator.Messaging;
using Beter.Feed.TestingSandbox.Emulator.Messaging.Handlers;
using Beter.Feed.TestingSandbox.Emulator.Messaging.Handlers.Abstract;
using Beter.Feed.TestingSandbox.Emulator.Messaging.Options;
using Beter.Feed.TestingSandbox.Emulator.Services;

namespace Beter.Feed.TestingSandbox.Emulator.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IConnectionManager, ConnectionManager>();

        return services;
    }

    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {

        services.Configure<MessagingOptions>(configuration.GetSection(MessagingOptions.SectionName));

        services.AddMessageHandlers();
        services.AddSingleton<IMessageHandlerResolver, MessageHandlerResolver>();
        services.AddSingleton<IConsumeMessageConverter, ConsumeMessageConverter>();
        services.AddTransient<IGeneratorMessagesConsumer, GeneratorMessagesConsumer>();
        services.AddHostedService<GeneratorMessagesListener>();

        return services;
    }

    private static IServiceCollection AddMessageHandlers(this IServiceCollection services)
    {
        services.AddTransient<IMessageHandler, ScoreboardMessageHandler>();
        services.AddTransient<IMessageHandler, IncidentMessageHandler>();
        services.AddTransient<IMessageHandler, TradingMessageHandler>();
        services.AddTransient<IMessageHandler, TimeTableMessageHandler>();
        services.AddTransient<IMessageHandler, SubscriptionRemovedMessageHandler>();
        services.AddTransient<IMessageHandler, GlobalMessageHandler>();
        services.AddTransient<IMessageHandler, HeartbeatMessageHandler>();

        return services;
    }
}
