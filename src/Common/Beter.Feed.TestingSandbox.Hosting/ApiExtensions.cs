using System.Text.Json;
using System.Text.Json.Serialization;
using Beter.Feed.TestingSandbox.Hosting.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Beter.Feed.TestingSandbox.Hosting;

public static class ApiExtensions
{
    public static IServiceCollection AddApiControllers(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<JsonOptions>? configureJson = null)
    {
        if (configureJson == null)
        {
            configureJson = static config =>
            {
                config.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                config.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            };
        }

        services.AddControllers()
            .AddJsonOptions(configureJson);

        services.AddEndpointsApiExplorer();
        services.AddSwagger(configuration);

        return services;
    }

    public static IApplicationBuilder UseApiControllers(this IApplicationBuilder app, IWebHostEnvironment env, bool devSwaggerOnly = true)
    {
        // Configure the HTTP request pipeline.
        app.UseSwagger(env, devSwaggerOnly);
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
        return app;
    }
}
