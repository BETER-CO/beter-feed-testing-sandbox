using Beter.Feed.TestingSandbox.Generator.Application.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace Beter.Feed.TestingSandbox.Generator.Host.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));

            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            switch (exception)
            {
                case DomainException domainException:
                    await ResponseExceptResponse(context, domainException, HttpStatusCode.BadRequest);
                    break;
                case RequiredEntityNotFoundException notFoundException:
                    await ResponseExceptResponse(context, notFoundException, HttpStatusCode.NotFound);
                    break;
                case ArgumentException argumentException:
                    await ResponseExceptResponse(context, argumentException, HttpStatusCode.BadRequest);
                    break;
                case ValidationException validationException:
                    await ResponseExceptResponse(context, validationException, HttpStatusCode.BadRequest, LogLevel.Warning);
                    break;
                default:
                    await ResponseExceptResponse(context, exception, HttpStatusCode.BadRequest);
                    break;
            }
        }

        private async Task ResponseExceptResponse(HttpContext httpContext, Exception exception, HttpStatusCode statusCode, LogLevel logLevel = LogLevel.Error)
        {
            _logger.Log(logLevel, exception, "Exception: {exception}", exception.Message);

            httpContext.Response.Clear();
            httpContext.Response.StatusCode = (int)statusCode;
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsync(JsonSerializer.Serialize(new { Error = exception.Message }));
        }
    }
}
