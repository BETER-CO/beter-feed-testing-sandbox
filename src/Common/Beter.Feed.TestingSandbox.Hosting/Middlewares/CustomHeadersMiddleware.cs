using Microsoft.AspNetCore.Http;

namespace Beter.Feed.TestingSandbox.Hosting.Middlewares;

public class CustomHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public CustomHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        if (httpContext == null)
        {
            throw new ArgumentNullException(nameof(httpContext));
        }

        httpContext.Response.Headers.Add("X-Frame-Options", "DENY");
        httpContext.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        httpContext.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        httpContext.Response.Headers.Add("Cache-Control", "no-cache");

        await _next(httpContext);
    }
}
