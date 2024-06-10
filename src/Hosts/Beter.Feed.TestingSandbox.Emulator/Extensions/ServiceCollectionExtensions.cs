using Beter.Feed.TestingSandbox.Emulator.Host.HostedServices;
using Beter.Feed.TestingSandbox.Emulator.Host.Options;
using Beter.Feed.TestingSandbox.Emulator.Messaging;
using Beter.Feed.TestingSandbox.Emulator.Messaging.Handlers;
using Beter.Feed.TestingSandbox.Emulator.Messaging.Handlers.Abstract;
using Beter.Feed.TestingSandbox.Emulator.Messaging.Options;
using Beter.Feed.TestingSandbox.Emulator.Services.Connections;
using Beter.Feed.TestingSandbox.Emulator.Services.Heartbeats;

namespace Beter.Feed.TestingSandbox.Emulator.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConnectionManager, ConnectionManager>();
        services.AddHeartbeatDependency(configuration);

        return services;
    }

    private static IServiceCollection AddHeartbeatDependency(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<HeartbeatOptions>(configuration.GetSection(HeartbeatOptions.SectionName));
        services.AddSingleton<IHeartbeatControlService, HeartbeatControlService>();
        services.AddHostedService<HeartbeatRunnerHostedService>();

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
        services.AddTransient<IMessageHandler, SteeringCommandMessageHandler>();

        return services;
    }
}
