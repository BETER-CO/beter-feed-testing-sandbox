using Beter.Feed.TestingSandbox.Generator.Application.Contracts;
using Beter.Feed.TestingSandbox.Generator.Application.Contracts.FeedConnections;
using Beter.Feed.TestingSandbox.Generator.Application.Services;
using Beter.Feed.TestingSandbox.Generator.Contracts.Requests;
using Beter.Feed.TestingSandbox.Generator.Infrastructure.Options;
using Beter.Feed.TestingSandbox.Generator.Infrastructure.Services;
using Beter.Feed.TestingSandbox.Generator.Infrastructure.Services.FeedConnections;
using FluentValidation;
using Polly;
using Polly.Extensions.Http;

namespace Beter.Feed.TestingSandbox.Generator.Infrastructure.Extensions;

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
        services.AddValidatorsFromAssembly(typeof(StartPlaybackRequestValidator).Assembly);

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