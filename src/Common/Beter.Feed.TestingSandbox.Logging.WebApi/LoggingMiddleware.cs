using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Serilog.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Beter.Feed.TestingSandbox.Logging.WebApi;

public class LoggingMiddleware
{
    public const string LoggingCorrelationIdName = "CorrelationId";

    private const string BeginMessageTemplate =
        "{RequestProtocol} {RequestMethod} {RequestPath}";

    private const string MessageTemplate =
        "{RequestProtocol} {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms.";

    private readonly ILogger<LoggingMiddleware> _logger;

    private readonly RequestDelegate _next;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? NullLogger<LoggingMiddleware>.Instance;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        if (httpContext == null)
        {
            throw new ArgumentNullException(nameof(httpContext));
        }

        using (_logger.BeginScope(
        new Dictionary<string, object?>
        { [LoggingCorrelationIdName] = CorrelationIdManager.CorrelationId }))
        {
            using var logContext =
                LogContext.Push(
                    new LogUserIdEnricher(httpContext.RequestServices.GetService<IHttpContextAccessor>()));

            var start = Stopwatch.GetTimestamp();
            try
            {
                _logger.Log(
                    LogLevel.Debug,
                    BeginMessageTemplate,
                    httpContext.Request.Protocol,
                    httpContext.Request.Method,
                    httpContext.Request.Path);

                await _next(httpContext);

                var elapsedMs = GetElapsedMilliseconds(start, Stopwatch.GetTimestamp());

                var statusCode = httpContext.Response?.StatusCode;
                var level = statusCode > 499 ? LogLevel.Error : LogLevel.Information;

                IDisposable? logScope = null;
                try
                {
                    if (level == LogLevel.Error)
                    {
                        logScope = EnrichWithContextForError(httpContext);
                    }

                    _logger.Log(
                        level,
                        MessageTemplate,
                        httpContext.Request.Protocol,
                        httpContext.Request.Method,
                        httpContext.Request.Path,
                        statusCode,
                        elapsedMs);
                }
                finally
                {
                    logScope?.Dispose();
                }
            }
            catch (Exception ex) when (LogException(
                httpContext,
                GetElapsedMilliseconds(start, Stopwatch.GetTimestamp()),
                ex))
            {
                // Never caught, because `LogException()` returns false.
            }
        }
    }

    private static double GetElapsedMilliseconds(long start, long stop)
    {
        return (stop - start) * 1000 / (double)Stopwatch.Frequency;
    }

    private bool LogException(HttpContext httpContext, double elapsedMs, Exception ex)
    {
        using (EnrichWithContextForError(httpContext))
        {
            _logger.LogError(
                ex,
                MessageTemplate,
                httpContext.Request.Protocol,
                httpContext.Request.Method,
                httpContext.Request.Path,
                (int)HttpStatusCode.InternalServerError,
            elapsedMs);
        }
        return false;
    }

    private IDisposable? EnrichWithContextForError(HttpContext httpContext)
    {
        var request = httpContext.Request;

        var scopeProperties = new Dictionary<string, object>
        {
            ["RequestHeaders"] = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
            ["RequestHost"] = request.Host,
        };

        if (request.HasFormContentType)
        {
            scopeProperties.Add("RequestForm", request.Form.ToDictionary(v => v.Key, v => v.Value.ToString()));
        }

        return _logger.BeginScope(scopeProperties);
    }
}
