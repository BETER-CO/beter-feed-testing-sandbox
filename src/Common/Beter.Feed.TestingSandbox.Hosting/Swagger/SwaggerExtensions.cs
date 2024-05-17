using Beter.Feed.TestingSandbox.Hosting.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Beter.Feed.TestingSandbox.Hosting.Swagger;

internal static class SwaggerExtensions
{
    internal static IServiceCollection AddSwagger(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGen();

        var swaggerConfig = configuration.GetSection("Swagger");
        if (!swaggerConfig.Exists())
        {
            return services;
        }

        var swaggerOptions = swaggerConfig.Get<SwaggerSettings>();
        services.AddSingleton(swaggerOptions!);

        return services;
    }

    internal static IApplicationBuilder UseSwagger(this IApplicationBuilder app, IWebHostEnvironment env, bool devSwaggerOnly = true)
    {
        if (IsEnabled(app, env, devSwaggerOnly))
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        return app;
    }

    private static bool IsEnabled(IApplicationBuilder app, IWebHostEnvironment env, bool devSwaggerOnly = true)
    {
        var swaggerOptions = app.ApplicationServices.GetService<SwaggerSettings>();
        var isSwaggerEnabled = swaggerOptions?.Enabled ?? devSwaggerOnly;

        return env.IsDevelopment() || isSwaggerEnabled;
    }
}

