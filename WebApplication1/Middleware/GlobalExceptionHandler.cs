using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using WebApplication1.DataAccess.Models;
using WebApplication1.Services.ExceptionServices;
using WebApplication1.Services.LogServices;
using WebApplication1.Utils.DTOs;
using WebApplication1.Utils.Exceptions;
using WebApplication1.Utils.Settings;
using ValidationException = WebApplication1.Utils.Exceptions.ValidationException;

namespace WebApplication1.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly IExceptionMessageService _messageService;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IWebHostEnvironment env,
        IExceptionMessageService messageService,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _env = env;
        _messageService = messageService;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // If response has already started, we can't modify it
        if (httpContext.Response.HasStarted)
        {
            _logger.LogError(exception, 
                "Exception occurred but response has already started. Exception: {Message}", 
                exception.Message);
            return false; // Let ASP.NET Core handle it
        }

        try
        {
            var errorDetails = MapExceptionToErrorDetails(exception);
            var errorResponse = CreateErrorResponse(httpContext, exception, errorDetails);

            // Log to database (best-effort, non-blocking)
            // Fire and forget - don't await to avoid blocking the response
            _ = LogExceptionAsync(httpContext, exception, errorDetails, cancellationToken);

            // Set response
            httpContext.Response.StatusCode = errorResponse.Status;
            httpContext.Response.ContentType = "application/json";

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _env.IsDevelopment()
            };

            await httpContext.Response.WriteAsJsonAsync(errorResponse, jsonOptions, cancellationToken);

            return true; // Exception handled
        }
        catch (Exception handlerException)
        {
            // Critical: Never throw exceptions from exception handler
            _logger.LogCritical(handlerException,
                "CRITICAL: GlobalExceptionHandler failed to handle exception. Original: {OriginalMessage}",
                exception.Message);

            // Try to send a basic error response if possible
            if (!httpContext.Response.HasStarted)
            {
                try
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    httpContext.Response.ContentType = "application/json";
                    await httpContext.Response.WriteAsJsonAsync(
                        new ErrorResponseDto
                        {
                            Status = 500,
                            Message = "An error occurred while processing your request.",
                            TraceId = httpContext.TraceIdentifier,
                            Timestamp = DateTime.UtcNow
                        },
                        cancellationToken: cancellationToken);
                }
                catch
                {
                    // If even this fails, just log - don't throw
                    _logger.LogCritical("Failed to send error response");
                }
            }

            return true; // Mark as handled to prevent further processing
        }
    }

    private (HttpStatusCode StatusCode, string Key, string SubKey, string? Type) MapExceptionToErrorDetails(Exception exception)
    {
        return exception switch
        {
            NotFoundException nf => 
                (HttpStatusCode.NotFound, nameof(NotFoundException), nf.SubKey, "NotFoundException"),
            
            ValidationException ve => 
                (HttpStatusCode.BadRequest, nameof(ValidationException), ve.SubKey, "ValidationException"),
            
            ConflictException ce => 
                (HttpStatusCode.Conflict, nameof(ConflictException), ce.SubKey, "ConflictException"),
            
            AppException ae => 
                (HttpStatusCode.BadRequest, nameof(AppException), ae.SubKey, "AppException"),
            
            UnauthorizedAccessException => 
                (HttpStatusCode.Unauthorized, "UnauthorizedAccessException", string.Empty, "UnauthorizedAccessException"),
            
            _ => 
                (HttpStatusCode.InternalServerError, "InternalServerError", string.Empty, "InternalServerError")
        };
    }

    private ErrorResponseDto CreateErrorResponse(
        HttpContext httpContext,
        Exception exception,
        (HttpStatusCode StatusCode, string Key, string SubKey, string? Type) errorDetails)
    {
        // Get localized message
        var preferredLanguage = GetPreferredLanguage(httpContext);
        var message = _messageService.GetMessage(errorDetails.Key, errorDetails.SubKey, preferredLanguage)
                     ?? exception.Message;

        // Create details object for development mode
        object? details = _env.IsDevelopment() ? new
        {
            ExceptionType = exception.GetType().Name,
            ExceptionMessage = exception.Message,
            StackTrace = exception.StackTrace,
            InnerException = exception.InnerException != null ? new
            {
                Type = exception.InnerException.GetType().Name,
                Message = exception.InnerException.Message
            } : null
        } : null;

        var response = new ErrorResponseDto
        {
            Status = (int)errorDetails.StatusCode,
            Message = message,
            Type = errorDetails.Type,
            TraceId = httpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow,
            Details = details
        };

        return response;
    }

    private string GetPreferredLanguage(HttpContext httpContext)
    {
        // Try to get language from Accept-Language header
        var acceptLanguage = httpContext.Request.Headers["Accept-Language"].ToString();
        
        if (acceptLanguage.Contains("sr", StringComparison.OrdinalIgnoreCase) ||
            acceptLanguage.Contains("sr-Latn", StringComparison.OrdinalIgnoreCase))
        {
            return "sr-Latn";
        }

        return "en-US";
    }

    private async Task LogExceptionAsync(
        HttpContext httpContext,
        Exception exception,
        (HttpStatusCode StatusCode, string Key, string SubKey, string? Type) errorDetails,
        CancellationToken cancellationToken)
    {
        // Use service scope factory to create a scope for scoped services
        // This is necessary because IExceptionHandler is singleton but ILogService is scoped
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var logService = scope.ServiceProvider.GetRequiredService<ILogService>();

            var controller = httpContext.Request.RouteValues["controller"]?.ToString() ?? "Unknown";
            var action = httpContext.Request.RouteValues["action"]?.ToString() ?? "Unknown";
            var userId = GetUserId(httpContext);

            var log = new Log
            {
                HappenedAtDate = DateTimeSettings.DateTimeBelgrade(),
                Process = controller,
                Activity = action,
                IpAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                Message = $"EXCEPTION [{errorDetails.Type}]: {exception.Message}",
                UserId = userId
            };

            await logService.CreateAsync(log);
        }
        catch (Exception logEx)
        {
            // Don't let logging failure affect the response
            _logger.LogError(logEx,
                "LogService failed to write exception log. Original exception: {OriginalMessage}",
                exception.Message);
        }
    }

    private static int GetUserId(HttpContext httpContext)
    {
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return 0;
    }
}
