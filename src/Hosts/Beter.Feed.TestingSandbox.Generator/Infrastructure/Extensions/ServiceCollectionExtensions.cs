using Beter.Feed.TestingSandbox.Generator.Application.Contracts;
using Beter.Feed.TestingSandbox.Generator.Application.Services;
using Beter.Feed.TestingSandbox.Generator.Contracts.Requests;
using Beter.Feed.TestingSandbox.Generator.Infrastructure.Options;
using Beter.Feed.TestingSandbox.Generator.Infrastructure.Services;
using FluentValidation;

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
}