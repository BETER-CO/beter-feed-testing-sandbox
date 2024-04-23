using Beter.TestingTools.Models.Incidents;
using Beter.TestingTools.Models.Scoreboards;
using Beter.TestingTools.Models.TimeTableItems;
using Beter.TestingTools.Models.TradingInfos;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json.Serialization;
using Beter.TestingTools.Emulator.SignalR.Settings;
using Beter.TestingTools.Emulator.SignalR.Filters;
using Beter.TestingTools.Emulator.Publishers.Extensions;
using Beter.TestingTools.Emulator.SignalR.Hubs.V1;
using Beter.TestingTools.Emulator.SignalR.Hubs;


namespace Beter.TestingTools.Emulator.Publishers.Extensions;

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
                builder.AllowAnyMethod().AllowAnyHeader().SetIsOriginAllowed((_) => true)
                    .AllowCredentials();
            });
        });

        var signalRServerBuilder = services.AddSignalR(delegate (HubOptions options)
        {
            options.EnableDetailedErrors = true;
            options.AddFilter<SignalRValidationFilter>(); //validate api key for connection
        }).AddJsonProtocol(delegate (JsonHubProtocolOptions cfg)
        {
            cfg.PayloadSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
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