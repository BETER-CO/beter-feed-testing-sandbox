using Beter.TestingTools.Generator.Application.Contracts;
using Beter.TestingTools.Generator.Application.Contracts.FeedConnections;
using Beter.TestingTools.Generator.Application.Services;
using Beter.TestingTools.Generator.Infrastructure.Options;
using Beter.TestingTools.Generator.Infrastructure.Services;
using Beter.TestingTools.Generator.Infrastructure.Services.FeedConnections;
using Polly;
using Polly.Extensions.Http;
using System.Diagnostics.CodeAnalysis;

namespace Beter.TestingTools.Generator.Infrastructure.Extensions;

static internal class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PublishOptions>(configuration.GetSection(PublishOptions.SectionName));

        services.AddSingleton<IPublisher, Publisher>();
        services.AddSingleton<ISequenceNumberProvider, SequenceNumberProvider>();
        services.AddSingleton<ISystemClock, SystemClock>();

        return services;
    }

    public static IServiceCollection AddFeedConnections(this IServiceCollection services, IConfiguration configuration)
    {
        var feedEmulatorOptions = new FeedEmulatorOptions();
        configuration.GetSection(FeedEmulatorOptions.SectionName).Bind(feedEmulatorOptions);
        services.Configure<FeedEmulatorOptions>(configuration.GetSection(FeedEmulatorOptions.SectionName));

        services.AddSingleton<IFeedEmulatorUrlProvider, FeedEmulatorUrlProvider>();
        services.AddHttpClient<IFeedConnectionService, FeedConnectionService>()
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
}