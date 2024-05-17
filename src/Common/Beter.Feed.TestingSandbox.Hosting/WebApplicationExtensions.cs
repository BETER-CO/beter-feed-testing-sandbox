using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using System.Net.Mime;
using Beter.Feed.TestingSandbox.Hosting.Middlewares;
using Beter.Feed.TestingSandbox.Logging.WebApi;

namespace Beter.Feed.TestingSandbox.Hosting;

public static class WebApplicationExtensions
{
    public static IApplicationBuilder UseWebApi(this IApplicationBuilder app)
    {
        app.UseMiddleware<LoggingMiddleware>();
        app.UseMiddleware<CustomHeadersMiddleware>();

        app.UseResponseCompression();
        return app;
    }

    public static IApplicationBuilder AddHealthCheckEndpoint(this IApplicationBuilder app)
    {
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHealthChecks("/health");
            endpoints.MapHealthChecks("/hc", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    var result = JsonConvert.SerializeObject(
                        new
                        {
                            status = report.Status.ToString(),
                            errors = report.Entries.Select(e => new { key = e.Key, value = Enum.GetName(typeof(HealthStatus), e.Value.Status), }),
                        });
                    context.Response.ContentType = MediaTypeNames.Application.Json;
                    await context.Response.WriteAsync(result);
                },
            });
        });

        return app;
    }
}

