using Beter.Feed.TestingSandbox.Server.Api;
using Beter.Feed.TestingSandbox.Server.Description;
using Beter.Feed.TestingSandbox.Server.Extensions;
using Beter.Feed.TestingSandbox.Server.Publishers.Extensions;

namespace Beter.Feed.TestingSandbox.Server;

public class Startup
{
    private IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSandboxApi();
        services.AddSignalR(Configuration);
        services.AddPublishers(Configuration);
        services.AddApplicationServices(Configuration);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();
        app.UseCors(FeedHubEndpoints.CorsPolicy);
        app.UseSwaggerUI(options =>
        {
            options.RoutePrefix = SandboxApi.SwaggerUiPath.TrimStart('/');
            options.SwaggerEndpoint(SandboxApi.OpenApiDocumentPath, OpenApiDocumentProvider.Title);
        });
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapSandboxApi();
            endpoints.MapFeedHubs(Configuration);
        });
    }
}
