namespace Beter.TestingTool.Generator.Host.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation("Http request: {Method} {Path}", context.Request.Method, context.Request.Path);

        // Call the next middleware in the pipeline
        await _next(context);
    }
}
