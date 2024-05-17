using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Beter.Feed.TestingSandbox.Emulator.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddFeedEmulatorSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Feed Emulator API",
                Version = "v1"
            });

            AddBasicAuthenticationDefinition(options);
        });

        return services;
    }

    public static IApplicationBuilder UseFeedEmulatorSwagger(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Feed Emulator API V1");
        });

        return app;
    }

    private static void AddBasicAuthenticationDefinition(SwaggerGenOptions options)
    {
        options.AddSecurityDefinition("basic", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "basic",
            In = ParameterLocation.Header,
            Description = "Basic Authorization header using the Bearer scheme."
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "basic"
                    }
                },
                Array.Empty<string>()
            }
        });
    }
}

