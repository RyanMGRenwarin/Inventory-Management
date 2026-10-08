using Serilog.Core;
using Serilog.Events;
using System.Security.Claims;

namespace InventoryManagement.Web.Logging;

/// <summary>
/// Enriches log events with the current user's ID and username.
/// </summary>
public class UserIdEnricher : ILogEventEnricher
{
    private static IHttpContextAccessor? _httpContextAccessor;

    /// <summary>
    /// Static method to set HttpContextAccessor (called from Program.cs).
    /// </summary>
    public static void Configure(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _httpContextAccessor?.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = httpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            var role = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;

            if (!string.IsNullOrEmpty(userId))
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserId", userId));

            if (!string.IsNullOrEmpty(username))
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Username", username));

            if (!string.IsNullOrEmpty(role))
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserRole", role));
        }
    }
}
