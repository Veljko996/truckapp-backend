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
        if (httpContext.Response.HasStarted)
        {
            _logger.LogError(exception,
                "Exception occurred but response has already started. Exception: {Message}",
                exception.Message);
            return false;
        }

        try
        {
            var errorDetails = MapExceptionToErrorDetails(exception);
            var errorResponse = CreateErrorResponse(httpContext, exception, errorDetails);

            _ = LogExceptionAsync(httpContext, exception, errorDetails, cancellationToken);

            httpContext.Response.StatusCode = errorResponse.Status;
            httpContext.Response.ContentType = "application/json";

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _env.IsDevelopment()
            };

            await httpContext.Response.WriteAsJsonAsync(errorResponse, jsonOptions, cancellationToken);

            return true;
        }
        catch (Exception handlerException)
        {
            _logger.LogCritical(handlerException,
                "CRITICAL: GlobalExceptionHandler failed. Original: {Message}",
                exception.Message);

            if (!httpContext.Response.HasStarted)
            {
                httpContext.Response.StatusCode = 500;
                httpContext.Response.ContentType = "application/json";
                await httpContext.Response.WriteAsJsonAsync(
                    new ErrorResponseDto
                    {
                        Status = 500,
                        Message = "An error occurred.",
                        TraceId = httpContext.TraceIdentifier,
                        Timestamp = DateTime.UtcNow
                    },
                    cancellationToken);
            }

            return true;
        }
    }

    private (HttpStatusCode StatusCode, string Key, string SubKey, string? Type)
        MapExceptionToErrorDetails(Exception exception)
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
        var preferredLanguage = GetPreferredLanguage(httpContext);
        var message = _messageService.GetMessage(errorDetails.Key, errorDetails.SubKey, preferredLanguage)
                     ?? exception.Message;

        object? details = _env.IsDevelopment()
            ? new
            {
                ExceptionType = exception.GetType().Name,
                ExceptionMessage = exception.Message,
                StackTrace = exception.StackTrace
            }
            : null;

        return new ErrorResponseDto
        {
            Status = (int)errorDetails.StatusCode,
            Message = message,
            Type = errorDetails.Type,
            TraceId = httpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow,
            Details = details
        };
    }

    private string GetPreferredLanguage(HttpContext httpContext)
    {
        var acceptLanguage = httpContext.Request.Headers["Accept-Language"].ToString();
        return acceptLanguage.Contains("sr", StringComparison.OrdinalIgnoreCase)
            ? "sr-Latn"
            : "en-US";
    }

    private async Task LogExceptionAsync(
        HttpContext httpContext,
        Exception exception,
        (HttpStatusCode StatusCode, string Key, string SubKey, string? Type) errorDetails,
        CancellationToken cancellationToken)
    {
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
                UserId = userId,

                RequestPath = httpContext.Request.Path + httpContext.Request.QueryString,
                RequestMethod = httpContext.Request.Method,
                HasAccessTokenCookie = httpContext.Request.Cookies.ContainsKey("accessToken"),
                HasRefreshTokenCookie = httpContext.Request.Cookies.ContainsKey("refreshToken")
            };

            await logService.CreateAsync(log);
        }
        catch (Exception logEx)
        {
            _logger.LogError(logEx, "Failed to write exception log");
        }
    }

    private static int GetUserId(HttpContext httpContext)
    {
        var claim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
    }
}
