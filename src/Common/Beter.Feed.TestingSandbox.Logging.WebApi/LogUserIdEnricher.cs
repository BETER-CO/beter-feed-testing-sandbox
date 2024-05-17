using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Beter.Feed.TestingSandbox.Logging.WebApi;

public class LogUserIdEnricher : ILogEventEnricher
{
    public const string UserIdPropertyName = "UserId";
    private readonly IHttpContextAccessor? _contextAccessor;
    private LogEventProperty? _cachedProperty;

    public LogUserIdEnricher(IHttpContextAccessor? contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public LogUserIdEnricher()
    {
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var userName = _contextAccessor?.HttpContext?.User?.Identity?.Name;

        if (!string.IsNullOrEmpty(userName))
        {
            _cachedProperty ??= propertyFactory.CreateProperty(UserIdPropertyName, userName);
        }

        if (_cachedProperty != null)
        {
            logEvent.AddOrUpdateProperty(_cachedProperty);
        }
    }
}
