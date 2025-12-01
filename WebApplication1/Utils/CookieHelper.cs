namespace WebApplication1.Utils;

public static class CookieHelper
{
    private const string AccessTokenCookieName = "accessToken";
    private const string RefreshTokenCookieName = "refreshToken";

    public static void SetTokenCookies(HttpResponse response, string accessToken, string refreshToken, bool isDevelopment = false)
    {
        // U productionu (Azure) MORA biti Secure=true za SameSite=None
        // SameSite=None zahteva Secure=true zbog browser security policy
        var baseOptions = new CookieOptions
        {
            HttpOnly = true, // Zaštita od XSS - JavaScript ne može pristupiti
            Secure = !isDevelopment, // U productionu (Azure) mora biti true
            SameSite = SameSiteMode.None, // Dozvoljava cross-site cookies (potrebno za Azure Static Web Apps)
            Path = "/",
            IsEssential = true // Dozvoljava cookie čak i ako korisnik blokira treće strane
        };

        var accessOptions = new CookieOptions
        {
            HttpOnly = baseOptions.HttpOnly,
            Secure = baseOptions.Secure,
            SameSite = baseOptions.SameSite,
            Path = baseOptions.Path,
            IsEssential = baseOptions.IsEssential,
            MaxAge = TimeSpan.FromMinutes(30)
        };

        response.Cookies.Append(AccessTokenCookieName, accessToken, accessOptions);

        var refreshOptions = new CookieOptions
        {
            HttpOnly = baseOptions.HttpOnly,
            Secure = baseOptions.Secure,
            SameSite = baseOptions.SameSite,
            Path = baseOptions.Path,
            IsEssential = baseOptions.IsEssential,
            MaxAge = TimeSpan.FromDays(7)
        };

        response.Cookies.Append(RefreshTokenCookieName, refreshToken, refreshOptions);
    }

    public static void DeleteTokenCookies(HttpResponse response, bool isDevelopment = false)
    {
        // Ista konfiguracija kao pri postavljanju - mora biti identična da bi browser obrisao cookie
        var deleteOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDevelopment, // Mora biti isti kao pri postavljanju
            SameSite = SameSiteMode.None, // Mora biti isti kao pri postavljanju
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
