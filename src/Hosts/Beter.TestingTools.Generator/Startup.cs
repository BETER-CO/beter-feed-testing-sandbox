using Beter.TestingTools.Hosting;
using Beter.TestingTool.Generator.Application.Extensions;
using Beter.TestingTool.Generator.Host.Common.ApplicationConfiguration.Extensions;
using Beter.TestingTool.Generator.Host.Extensions;
using Beter.TestingTool.Generator.Host.Middlewares;
using Beter.TestingTool.Generator.Infrastructure.Extensions;

namespace Beter.TestingTool.Generator;

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

        app.UseRouting();
        app.UseGeneratorSwagger();

        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseEndpoints<Program>();
        app.AddHealthCheckEndpoint();
        app.ConfigureMaxRequestBodySize();
    }
}