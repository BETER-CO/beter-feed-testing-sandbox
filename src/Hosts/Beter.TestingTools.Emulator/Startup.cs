using Beter.TestingTools.Hosting;
using Beter.TestingTools.Emulator.Extensions;
using Beter.TestingTools.Emulator.Publishers.Extensions;

namespace Beter.TestingTools.Emulator;

public class Startup
{
    private IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMessaging(Configuration);
        services.AddEndpointsApiExplorer();
        services.AddFeedEmulatorSwagger();
        services.AddSignalR(Configuration);
        services.AddPublishers(Configuration);
        services.AddApplicationServices();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();
        app.UseFeedEmulatorSwagger();
        app.AddHealthCheckEndpoint();
        app.UseEndpoints<Program>();
        app.UseSignalR(Configuration);
    }
}