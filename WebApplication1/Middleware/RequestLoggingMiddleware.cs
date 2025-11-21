using System.Diagnostics;
using System.Security.Claims;
using WebApplication1.Services.LogServices;

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

        int? userId = null;
        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim != null && int.TryParse(claim.Value, out var parsedId))
            userId = parsedId;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Don't re-throw - let ErrorHandlerMiddleware handle it
            // Only log if response hasn't been handled yet
            if (!context.Response.HasStarted)
            {
                _logger.LogError(ex, "Neobrađena greška u pipeline-u: {Message}", ex.Message);
                throw; // Re-throw only if response hasn't started
            }
            // If response has started, ErrorHandlerMiddleware already handled it
        }
        finally
        {
            sw.Stop();

            // Ignoriši interne rute — ali BEZ return, samo preskoči logovanje
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
                    var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    var status = context.Response.StatusCode;

                    if (controller != "Auth" || action != "Login")
                    {
                        var log = new Log
                        {
                            Process = controller,
                            Activity = action,
                            Message = $"[{context.Request.Method}] {context.Request.Path} → {status} ({sw.ElapsedMilliseconds} ms)",
                            UserId = userId ?? 0,
                            IpAddress = ipAddress
                        };

                        try
                        {
                            await logService.CreateAsync(log);
                        }
                        catch (Exception dbEx)
                        {
                            _logger.LogWarning(dbEx, "Neuspešno upisivanje loga u bazu (RequestLoggingMiddleware)");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "RequestLoggingMiddleware — greška pri kreiranju loga");
                }
            }
        }
    }
}
