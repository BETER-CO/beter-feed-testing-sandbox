using Beter.Feed.TestingSandbox.Hosting;
using Beter.Feed.TestingSandbox.Generator.Host.Extensions;
using Beter.Feed.TestingSandbox.Generator.Infrastructure.Extensions;
using Beter.Feed.TestingSandbox.Generator.Host.Middlewares;
using Beter.Feed.TestingSandbox.Generator.Host.Common.ApplicationConfiguration.Extensions;
using Beter.Feed.TestingSandbox.Generator.Application.Extensions;

namespace Beter.Feed.TestingSandbox.Generator;

public class Startup
{
    private IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddEndpointsApiExplorer()
            .AddGeneratorSwagger()
            .AddInfrastructure(Configuration)
            .AddApplicationServices(Configuration)
            .AddFeedConnections(Configuration);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseMiddleware<ExceptionMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();

        app.UseRouting();
        app.UseGeneratorSwagger();
        app.UseEndpoints<Program>();
        app.AddHealthCheckEndpoint();
        app.ConfigureMaxRequestBodySize();
    }
}