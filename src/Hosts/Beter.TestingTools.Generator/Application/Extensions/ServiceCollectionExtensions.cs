using Beter.TestingTool.Generator.Infrastructure.Repositories;
using Beter.TestingTool.Generator.Application.Contracts;
using Beter.TestingTool.Generator.Application.Contracts.Heartbeats;
using Beter.TestingTool.Generator.Application.Contracts.Playbacks;
using Beter.TestingTool.Generator.Application.Contracts.TestScenarios;
using Beter.TestingTool.Generator.Application.Services;
using Beter.TestingTool.Generator.Application.Services.Heartbeats;
using Beter.TestingTool.Generator.Application.Services.Playbacks;
using Beter.TestingTool.Generator.Application.Services.Playbacks.Transformations;
using Beter.TestingTool.Generator.Application.Services.Playbacks.Transformations.Rules;
using Beter.TestingTool.Generator.Application.Services.TestScenarios;
using Beter.TestingTool.Generator.Application.Services.TestScenarios.MessageHandlers;
using Beter.TestingTool.Generator.Domain.TestScenarios;
using Beter.TestingTool.Generator.Host.HostedServices;
using Beter.TestingTool.Generator.Host.Options;

namespace Beter.TestingTool.Generator.Application.Extensions;

static internal class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddPlaybackDependency()
            .AddTestScenariosDependency()
            .AddHeartbeatDependency(configuration);
    }

    private static IServiceCollection AddTestScenariosDependency(this IServiceCollection services)
    {
        services.AddSingleton<ITestScenarioMessageHandlerResolver, TestScenarioMessageHandlerResolver>();
        services.AddSingleton<ITestScenarioMessageHandler, SteeringCommandMessageHandler>();
        services.AddSingleton<ITestScenarioMessageHandler, FeedMessageHandler>();
        services.AddSingleton<ITestScenarioMessageHandler, DefaultMessageHandler>();

        services.AddSingleton<IPlaybackScheduler, PlaybackScheduler>();
        services.AddHostedService<TestScenarioRunnerHostedService>();
        services.AddSingleton<ITestScenarioService, TestScenarioService>();
        services.AddSingleton<ITestScenarioFactory, TestScenarioFactory>();
        services.AddSingleton<ITestScenariosRepository>(sp =>
        {
            var factory = sp.GetRequiredService<ITestScenarioFactory>();
            var scenarios = factory.Create(TestScenario.ResourcesPath);

            return new InMemoryTestScenariosRepository(scenarios);
        });

        return services;
    }

    private static IServiceCollection AddHeartbeatDependency(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<HeartbeatOptions>(configuration.GetSection(HeartbeatOptions.SectionName));
        services.AddSingleton<IHeartbeatControlService, HeartbeatControlService>();
        services.AddHostedService<HeartbeatRunnerHostedService>();

        return services;
    }

    private static IServiceCollection AddPlaybackDependency(this IServiceCollection services)
    {
        services.AddSingleton<IPlaybackFactory, PlaybackFactory>();
        services.AddSingleton<IPlaybackRepository, InMemoryPlaybacksRepository>();

        services.AddSingleton<ITransformationManager, TransformationManager>();
        services.AddSingleton<ITransformationRule, IncidentTransformationRule>();
        services.AddSingleton<ITransformationRule, TradingTransformationRule>();
        services.AddSingleton<ITransformationRule, ScoreboardTransformationRule>();
        services.AddSingleton<ITransformationRule, TimeTableTransformationRule>();
        services.AddSingleton<ITransformationRule, SubscriptionsRemovedTransformationRule>();
        services.AddSingleton<ITransformationRule, SystemEventTransformationRule>();

        services.AddSingleton<IMessagesTransformationContextFactory, MessagesTransformationContextFactory>();
        services.AddSingleton<IRunCountTracker, RunCountTracker>();
        services.AddSingleton<IMatchIdGenerator, MatchIdGenerator>();

        return services;
    }
}