using Beter.Feed.TestingSandbox.Consumer.Extensions;
using Beter.Feed.TestingSandbox.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Beter.Feed.TestingSandbox.Consumer;

public class Startup
{
    private IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddKafkaConfiguration(Configuration);
        services.AddHostedServices();
        services.AddEndpointsApiExplorer();
        services.AddFeedConsumerSwagger();
        services.AddFeedServiceClients();
        services.AddFeedMessageProducers();
        services.AddApplicationServices(Configuration);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();
        app.UseFeedConsumerSwagger();
        app.AddHealthCheckEndpoint();
        app.UseEndpoints<Program>();
    }
}