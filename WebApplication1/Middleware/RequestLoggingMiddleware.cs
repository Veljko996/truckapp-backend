using System.Diagnostics;
using System.Security.Claims;
using WebApplication1.DataAccess.Models;
using WebApplication1.Services.LogServices;
using WebApplication1.Utils.Settings;

namespace WebApplication1.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ILogService logService)
    {
        var sw = Stopwatch.StartNew();

        int userId = 0;
        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim != null && int.TryParse(claim.Value, out var parsedId))
            userId = parsedId;

        try
        {
            await _next(context);
        }
        catch
        {
            throw;
        }
        finally
        {
            sw.Stop();

            bool skip =
                context.Request.Path.StartsWithSegments("/swagger") ||
                context.Request.Path.StartsWithSegments("/favicon") ||
                context.Request.Path.StartsWithSegments("/health");

            if (!skip)
            {
                try
                {
                    var controller = context.Request.RouteValues["controller"]?.ToString() ?? "Unknown";
                    var action = context.Request.RouteValues["action"]?.ToString() ?? "Unknown";

                    if (controller != "Auth" || action != "Login")
                    {
                        var log = new Log
                        {
                            HappenedAtDate = DateTimeSettings.DateTimeBelgrade(),
                            Process = controller,
                            Activity = action,
                            Message =
                                $"[{context.Request.Method}] {context.Request.Path} → {context.Response.StatusCode} ({sw.ElapsedMilliseconds} ms)",
                            UserId = userId,
                            IpAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown",

                            RequestPath = context.Request.Path + context.Request.QueryString,
                            RequestMethod = context.Request.Method,
                            HasAccessTokenCookie = context.Request.Cookies.ContainsKey("accessToken"),
                            HasRefreshTokenCookie = context.Request.Cookies.ContainsKey("refreshToken")
                        };

                        await logService.CreateAsync(log);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "RequestLoggingMiddleware failed to write log");
                }
            }
        }
    }
}
