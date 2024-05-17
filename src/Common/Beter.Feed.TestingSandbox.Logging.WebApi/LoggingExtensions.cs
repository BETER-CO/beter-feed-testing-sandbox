using Beter.Feed.TestingSandbox.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Beter.Feed.TestingSandbox.Logging.WebApi;

public static class LoggingExtensions
{
    public static ILoggerFactory AddLogging(this ILoggerFactory loggerFactory)
    {
        return loggerFactory.AddSerilog(Logger.Instance);
    }

    public static IHostBuilder AddLogging(this IHostBuilder builder)
    {
        return builder.UseSerilog(Logger.Instance);
    }
}
