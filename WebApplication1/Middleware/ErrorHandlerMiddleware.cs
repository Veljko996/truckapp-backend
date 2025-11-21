namespace WebApplication1.Middleware;

public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlerMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ErrorHandlerMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlerMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context, ILogService logService)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, logService);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex, ILogService logService)
    {
        try
        {
            // If response has already started, we can't modify it - just log and return
            if (context.Response.HasStarted)
            {
                _logger.LogError(ex, "Exception occurred but response has already started. Exception: {Message}", ex.Message);
                return;
            }

            // Clear any existing response
            context.Response.Clear();
            context.Response.ContentType = "application/json";

            var log = new Log
            {
                HappenedAtDate = DateTimeSettings.DateTimeBelgrade(),
                Process = context.Request.RouteValues["controller"]?.ToString() ?? "Unknown",
                Activity = context.Request.RouteValues["action"]?.ToString() ?? "Unknown",
                IpAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                Message = $"EXCEPTION: {ex.Message}",
                UserId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) is { } claim
                        && int.TryParse(claim.Value, out var uid) ? uid : 0
            };

            // mapiranje statusa
            var (statusCode, key, subKey) = ex switch
            {
                NotFoundException nf => (HttpStatusCode.NotFound, nameof(NotFoundException), nf.SubKey),
                ValidationException ve => (HttpStatusCode.BadRequest, nameof(ValidationException), ve.SubKey),
                ConflictException ce => (HttpStatusCode.Conflict, nameof(ConflictException), ce.SubKey),
                AppException ae => (HttpStatusCode.BadRequest, nameof(AppException), ae.SubKey),
                UnauthorizedAccessException _ => (HttpStatusCode.Unauthorized, "UnauthorizedAccessException", string.Empty),
                _ => (HttpStatusCode.InternalServerError, "InternalServerError", string.Empty)
            };

            context.Response.StatusCode = (int)statusCode;

            // poruka iz JSON resursa (ako postoji)
            string message = await GetMessageFromJsonAsync(key, subKey) ?? ex.Message;

            // log u bazu (best-effort) - don't let logging failure block the response
            try 
            { 
                await logService.CreateAsync(log); 
            } 
            catch (Exception logEx) 
            { 
                _logger.LogError(logEx, "LogService nije uspeo da upiše log. Originalna greška: {OriginalMessage}", ex.Message); 
            }

            var response = new { status = (int)statusCode, message };
            var jsonResponse = JsonSerializer.Serialize(response,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            
            await context.Response.WriteAsync(jsonResponse);
        }
        catch (Exception handlerEx)
        {
            // Critical: Never throw exceptions from error handler
            _logger.LogCritical(handlerEx, 
                "CRITICAL: ErrorHandlerMiddleware failed to handle exception. Original: {OriginalMessage}", 
                ex.Message);
            
            // Try to send a basic error response if possible
            if (!context.Response.HasStarted)
            {
                try
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(
                        JsonSerializer.Serialize(new { status = 500, message = "An error occurred while processing your request." },
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
                }
                catch
                {
                    // If even this fails, just log - don't throw
                    _logger.LogCritical("Failed to send error response");
                }
            }
        }
    }

    private async Task<string?> GetMessageFromJsonAsync(string key, string subKey)
    {
        try
        {
            var path = Path.Combine(_env.ContentRootPath, "Resources", "exceptionMessages.json");
            if (!File.Exists(path)) return null;

            var json = await File.ReadAllTextAsync(path);
            var list = JsonSerializer.Deserialize<List<ExceptionMessage>>(json);
            var msg = list?.FirstOrDefault(x => x.Key == key && x.SubKey == subKey);
            return msg?.Message?.Sr ?? msg?.Message?.Eng;
        }
        catch
        {
            return null;
        }
    }
}
