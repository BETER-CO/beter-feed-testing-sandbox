using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using MessagePack;
using MessagePack.Resolvers;
using System.Text.Json.Serialization;
using Beter.Feed.TestingSandbox.Models.Scoreboards;
using Beter.Feed.TestingSandbox.Models.Incidents;
using Beter.Feed.TestingSandbox.Models.TradingInfos;
using Beter.Feed.TestingSandbox.Models.TimeTableItems;
using Beter.Feed.TestingSandbox.Emulator.SignalR.Filters;
using Beter.Feed.TestingSandbox.Emulator.Publishers.Extensions;
using Beter.Feed.TestingSandbox.Emulator.SignalR.Settings;
using Beter.Feed.TestingSandbox.Emulator.SignalR.Hubs.V1;
using Beter.Feed.TestingSandbox.Emulator.SignalR.Hubs;


namespace Beter.Feed.TestingSandbox.Emulator.Publishers.Extensions;

static internal class ServiceCollectionExtensions
{
    public static IServiceCollection AddPublishers(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SignalRSettings>(configuration.GetSection(SignalRSettings.SECTION_NAME));

        services.AddSingleton<IFeedMessagePublisherResolver, FeedMessagePublisherResolver>();
        services.AddSingleton<
            IMessagePublisher,
            IMessagePublisher<TimeTableItemModel>,
            FeedMessagePublisher<TimeTableHub, TimeTableItemModel, IFeedHub<TimeTableItemModel>>>();

        services.AddSingleton<
           IMessagePublisher,
           IMessagePublisher<TradingInfoModel>,
           FeedMessagePublisher<TradingHub, TradingInfoModel, IFeedHub<TradingInfoModel>>>();

        services.AddSingleton<
           IMessagePublisher,
           IMessagePublisher<ScoreBoardModel>,
           FeedMessagePublisher<ScoreboardHub, ScoreBoardModel, IFeedHub<ScoreBoardModel>>>();

        services.AddSingleton<
           IMessagePublisher,
           IMessagePublisher<IncidentModel>,
           FeedMessagePublisher<IncidentHub, IncidentModel, IFeedHub<IncidentModel>>>();

        return services;
    }

    public static IServiceCollection AddSignalR(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SignalRSettings>(configuration.GetSection("SignalR"));

        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddCors(delegate (CorsOptions o)
        {
            o.AddPolicy("SignalR", delegate (CorsPolicyBuilder builder)
            {
                builder.AllowAnyMethod()
                    .AllowAnyHeader()
                    .SetIsOriginAllowed((_) => true)
                    .AllowCredentials();
            });
        });

        var signalRServerBuilder = services.AddSignalR(delegate (HubOptions options)
        {
            options.EnableDetailedErrors = true;
            options.AddFilter<SignalRValidationFilter>();
        }).AddJsonProtocol(delegate (JsonHubProtocolOptions cfg)
        {
            cfg.PayloadSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        }).AddMessagePackProtocol(static options =>
        {
            StaticCompositeResolver.Instance.Register(ContractlessStandardResolver.Instance);
            options.SerializerOptions = MessagePackSerializerOptions.Standard.WithResolver(StaticCompositeResolver.Instance);
        });

        return services;
    }

    public static IServiceCollection AddSingleton<TService1, TService2, TImplementation>(this IServiceCollection services)
         where TImplementation : class, TService1, TService2
         where TService1 : class
         where TService2 : class
    {
        services.AddSingleton<TImplementation>();
        services.AddSingleton<TService1, TImplementation>(x => x.GetService<TImplementation>());
        services.AddSingleton<TService2, TImplementation>(x => x.GetService<TImplementation>());

        return services;
    }
}