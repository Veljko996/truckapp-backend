using Microsoft.AspNetCore.Http;

namespace WebApplication1.Utils;

public static class CookieHelper
{
    private const string AccessTokenCookieName = "accessToken";
    private const string RefreshTokenCookieName = "refreshToken";

   
    public static void SetTokenCookies(HttpResponse response, string accessToken, string refreshToken, bool isDevelopment = false)
    {
        var accessTokenOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDevelopment,   // MUST be true on Azure
            SameSite = SameSiteMode.None, // REQUIRED for cross-site cookies
            Path = "/",
            IsEssential = true,
            MaxAge = TimeSpan.FromMinutes(30)
        };
        response.Cookies.Append("accessToken", accessToken, accessTokenOptions);

        var refreshTokenOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDevelopment,
            SameSite = SameSiteMode.None, // REQUIRED
            Path = "/",
            IsEssential = true,
            MaxAge = TimeSpan.FromDays(7)
        };
        response.Cookies.Append("refreshToken", refreshToken, refreshTokenOptions);
    }

    /// Reads access token from cookie or Authorization header (fallback)
    
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

    /// Reads refresh token from cookie
    public static string? GetRefreshToken(HttpRequest request)
    {
        return request.Cookies[RefreshTokenCookieName];
    }

    /// Deletes both access and refresh token cookies
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

