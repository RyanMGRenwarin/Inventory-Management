using Serilog.Core;
using Serilog.Events;

namespace InventoryManagement.Web.Logging;

/// <summary>
/// Enriches log events with a correlation ID for the current request.
/// </summary>
public class RequestIdEnricher : ILogEventEnricher
{
    private static IHttpContextAccessor? _httpContextAccessor;

    public static void Configure(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _httpContextAccessor?.HttpContext;
        if (httpContext != null)
        {
            var correlationId = httpContext.TraceIdentifier;
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RequestId", correlationId));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RequestMethod", httpContext.Request.Method));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RequestPath", httpContext.Request.Path.Value));
        }
    }
}
