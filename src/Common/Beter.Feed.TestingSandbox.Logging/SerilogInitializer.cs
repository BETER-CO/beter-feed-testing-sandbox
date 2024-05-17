using Microsoft.Extensions.Configuration;
using Serilog.Debugging;
using Serilog.Events;
using Serilog;
using System.Net;
using Serilog.Formatting.Compact;
using Serilog.Sinks.Network;

namespace Beter.Feed.TestingSandbox.Logging;

internal static class SerilogInitializer
{
    public static Serilog.Core.Logger InitializeSerilog(IConfiguration configuration, string env, string app, string service)
    {
        var settings = GetSettings(configuration);
        if (settings.EnableSelfLog)
        {
            SelfLog.Enable(Console.Error);
        }

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Is(settings.MinimumLogLevel)
            .Enrich.FromLogContext()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("Application", app)
            .Enrich.WithProperty("Environment", env)
            .Enrich.WithProperty("Service", service)
            .Enrich.WithMachineName()
            .WithConsole(settings)
            .WithFile(settings, app)
            .WithTcp(settings)
            .WithHttp(settings);

        return loggerConfiguration.CreateLogger();
    }

    private static LoggerSettings GetSettings(IConfiguration configuration)
    {
        var configSection = configuration.GetSection("Logger");
        var loggerSettings = new LoggerSettings();
        if (configSection.Exists())
        {
            configSection.Bind(loggerSettings);
        }

        return loggerSettings;
    }

    private static LoggerConfiguration WithConsole(this LoggerConfiguration loggerConfiguration, LoggerSettings settings)
    {
        var sink = settings.Sinks.Console;
        if (sink == null || sink.Disabled)
        {
            return loggerConfiguration;
        }

        return loggerConfiguration.WriteTo.Async(
            config =>
            {
                if (sink.UseCompactJson)
                {
                    config.Console(new CompactJsonFormatter(), restrictedToMinimumLevel: sink.LogLevel);
                }
                else
                {
                    config.Console(outputTemplate: sink.Template, restrictedToMinimumLevel: sink.LogLevel);
                }
            },
            settings.AsyncQueueLength);
    }

    private static LoggerConfiguration WithFile(
        this LoggerConfiguration loggerConfiguration,
        LoggerSettings settings,
        string appName)
    {
        var sink = settings.Sinks.File;
        if (sink == null || sink.Disabled)
        {
            return loggerConfiguration;
        }

        loggerConfiguration = loggerConfiguration.WriteTo.File(
            $"./logs/{appName}-.log",
            rollingInterval: RollingInterval.Day,
            rollOnFileSizeLimit: true,
            fileSizeLimitBytes: sink.MaxFileSize,
            outputTemplate: sink.Template,
            restrictedToMinimumLevel: sink.LogLevel);

        return loggerConfiguration.WriteTo.File(
            $"./logs/{appName}.ERRORS-.log",
            rollingInterval: RollingInterval.Day,
            rollOnFileSizeLimit: true,
            fileSizeLimitBytes: sink.MaxFileSize,
            outputTemplate: sink.Template,
            restrictedToMinimumLevel: LogEventLevel.Error);
    }

    private static LoggerConfiguration WithHttp(
        this LoggerConfiguration loggerConfiguration,
        LoggerSettings settings)
    {
        var sink = settings.Sinks.Http;
        if (sink == null || sink.Disabled)
        {
            return loggerConfiguration;
        }

        return loggerConfiguration.WriteTo.Http(
            sink.RequestUri!,
            sink.QueueLimitBytes,
        sink.LogEventsInBatchLimit,
        sink.BatchSizeLimitBytes,
            restrictedToMinimumLevel: sink.LogLevel);
    }

    private static LoggerConfiguration WithTcp(this LoggerConfiguration loggerConfiguration, LoggerSettings settings)
    {
        var sink = settings.Sinks.Tcp;

        if (sink == null || sink.Disabled)
        {
            return loggerConfiguration;
        }

        if (string.IsNullOrEmpty(sink.Address))
        {
            throw new ArgumentException("Address in config file not specified", nameof(settings));
        }

        if (sink.Port.GetValueOrDefault() == 0)
        {
            throw new ArgumentException("Port in config file not specified", nameof(settings));
        }

        if (IPAddress.TryParse(sink.Address, out var ip))
        {
            return loggerConfiguration.WriteTo.TCPSink(ip, sink.Port.GetValueOrDefault());
        }

        if (Uri.IsWellFormedUriString(sink.Address, UriKind.Absolute))
        {
            return loggerConfiguration.WriteTo.TCPSink(sink.Address, sink.Port.GetValueOrDefault());
        }

        var tcpAddress = $"tcp://{sink.Address}";

        if (Uri.IsWellFormedUriString(tcpAddress, UriKind.Absolute))
        {
            return loggerConfiguration.WriteTo.TCPSink(tcpAddress, sink.Port.GetValueOrDefault());
        }

        throw new ArgumentException($"{tcpAddress} is not a valid URI", nameof(settings));
    }
}

