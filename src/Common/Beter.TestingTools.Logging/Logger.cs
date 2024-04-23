using Microsoft.Extensions.Configuration;
using Serilog;

namespace Beter.TestingTools.Logging;

public static class Logger
{
    public static ILogger Instance
    {
        get
        {
            return Log.Logger;
        }
    }

    public static void Initialize(IConfiguration configuration, string env, string app, string service)
    {
        Log.Logger = SerilogInitializer.InitializeSerilog(configuration, env, app, service);
    }

    public static void Close()
    {
        Log.CloseAndFlush();
    }
}
