namespace WebApplication1.Utils;

public static class CookieHelper
{
    private const string AccessTokenCookieName = "accessToken";
    private const string RefreshTokenCookieName = "refreshToken";

    public static void SetTokenCookies(HttpResponse response, string accessToken, string refreshToken, bool isDevelopment = false)
    {
        
        
        var baseOptions = new CookieOptions
        {
            HttpOnly = true, 
            Secure = !isDevelopment,  // u azure je true
            SameSite = SameSiteMode.None, // Dozvoljava cross-site cookies (potrebno za Azure Static Web Apps)
            Path = "/",
            IsEssential = true 
        };

        var accessOptions = new CookieOptions
        {
            HttpOnly = baseOptions.HttpOnly,
            Secure = baseOptions.Secure,
            SameSite = baseOptions.SameSite,
            Path = baseOptions.Path,
            IsEssential = baseOptions.IsEssential,
            Expires = DateTime.UtcNow.AddMinutes(30)
        };

        response.Cookies.Append(AccessTokenCookieName, accessToken, accessOptions);

        var refreshOptions = new CookieOptions
        {
            HttpOnly = baseOptions.HttpOnly,
            Secure = baseOptions.Secure,
            SameSite = baseOptions.SameSite,
            Path = baseOptions.Path,
            IsEssential = baseOptions.IsEssential,
            Expires = DateTime.UtcNow.AddHours(11)
        };

        response.Cookies.Append(RefreshTokenCookieName, refreshToken, refreshOptions);
    }

    public static void DeleteTokenCookies(HttpResponse response, bool isDevelopment = false)
    {
        
        var deleteOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDevelopment, 
            SameSite = SameSiteMode.None, 
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(-1) // Postavi u prošlost da bi browser obrisao
        };

        response.Cookies.Delete(AccessTokenCookieName, deleteOptions);
        response.Cookies.Delete(RefreshTokenCookieName, deleteOptions);
    }

    public static string? GetAccessToken(HttpRequest request)
    {
        return request.Cookies[AccessTokenCookieName];
    }

    public static string? GetRefreshToken(HttpRequest request)
    {
        return request.Cookies[RefreshTokenCookieName];
    }
}
