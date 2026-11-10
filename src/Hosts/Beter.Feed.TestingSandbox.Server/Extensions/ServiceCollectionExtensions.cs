using Beter.Feed.TestingSandbox.Server.Host.HostedServices;
using Beter.Feed.TestingSandbox.Server.Host.Options;
using Beter.Feed.TestingSandbox.Server.Services.Connections;
using Beter.Feed.TestingSandbox.Server.Services.Heartbeats;

namespace Beter.Feed.TestingSandbox.Server.Extensions;

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
}
