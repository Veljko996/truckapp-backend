using Microsoft.AspNetCore.Http;

namespace WebApplication1.Utils;

public static class CookieHelper
{
    private const string AccessTokenCookieName = "accessToken";
    private const string RefreshTokenCookieName = "refreshToken";

    /// <summary>
    /// Sets HTTP-only cookies for access and refresh tokens
    /// </summary>
    public static void SetTokenCookies(HttpResponse response, string accessToken, string refreshToken, bool isDevelopment = false)
    {
        // Access token - short-lived (30 minutes)
        var accessTokenOptions = new CookieOptions
        {
            HttpOnly = true, // JavaScript cannot access (XSS protection)
            Secure = !isDevelopment, // HTTPS only in production
            SameSite = SameSiteMode.Strict, // CSRF protection
            Path = "/", // Available for all paths
            IsEssential = true, // Required for GDPR compliance
            MaxAge = TimeSpan.FromMinutes(30)
        };
        response.Cookies.Append(AccessTokenCookieName, accessToken, accessTokenOptions);

        // Refresh token - longer-lived (7 days)
        var refreshTokenOptions = new CookieOptions
        {
            HttpOnly = true, // JavaScript cannot access (XSS protection)
            Secure = !isDevelopment, // HTTPS only in production
            SameSite = SameSiteMode.Strict, // CSRF protection
            Path = "/", // Available for all paths
            IsEssential = true, // Required for GDPR compliance
            MaxAge = TimeSpan.FromDays(7)
        };
        response.Cookies.Append(RefreshTokenCookieName, refreshToken, refreshTokenOptions);
    }

    /// <summary>
    /// Reads access token from cookie or Authorization header (fallback)
    /// </summary>
    public static string? GetAccessToken(HttpRequest request)
    {
        // Try cookie first (primary method)
        var tokenFromCookie = request.Cookies[AccessTokenCookieName];
        if (!string.IsNullOrWhiteSpace(tokenFromCookie))
        {
            return tokenFromCookie;
        }

        // Fallback to Authorization header for backward compatibility
        var authHeader = request.Headers["Authorization"].ToString();
        if (!string.IsNullOrWhiteSpace(authHeader) && 
            authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authHeader["Bearer ".Length..].Trim();
        }

        return null;
    }

    /// <summary>
    /// Reads refresh token from cookie
    /// </summary>
    public static string? GetRefreshToken(HttpRequest request)
    {
        return request.Cookies[RefreshTokenCookieName];
    }

    /// <summary>
    /// Deletes both access and refresh token cookies
    /// </summary>
    public static void DeleteTokenCookies(HttpResponse response, bool isDevelopment = false)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDevelopment,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(-1) // Set to past date to delete
        };

        response.Cookies.Delete(AccessTokenCookieName, cookieOptions);
        response.Cookies.Delete(RefreshTokenCookieName, cookieOptions);
    }
}

