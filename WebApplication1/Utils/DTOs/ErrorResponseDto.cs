namespace WebApplication1.Utils.DTOs;

/// <summary>
/// Standardized error response following RFC 7807 Problem Details format
/// </summary>
public class ErrorResponseDto
{
    /// <summary>
    /// HTTP status code
    /// </summary>
    public int Status { get; init; }

    /// <summary>
    /// Human-readable error message
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Error type/classification (e.g., "NotFoundException", "ValidationException")
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// Additional error details (only in development)
    /// </summary>
    public object? Details { get; init; }

    /// <summary>
    /// Request trace identifier for correlation
    /// </summary>
    public string? TraceId { get; init; }

    /// <summary>
    /// Timestamp when the error occurred
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
