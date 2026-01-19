namespace WebApplication1.Utils;

public static class CookieHelper
{
	private const string AccessTokenCookieName = "accessToken";
	private const string RefreshTokenCookieName = "refreshToken";

	public static void SetTokenCookies(
	HttpResponse response,
	string accessToken,
	string refreshToken,
	IWebHostEnvironment env)
	{
		var isDev = env.IsDevelopment();

		var sameSite = isDev ? SameSiteMode.None : SameSiteMode.None;
		var secure = !isDev;

		response.Cookies.Append(
			"accessToken",
			accessToken,
			new CookieOptions
			{
				HttpOnly = true,
				Secure = secure,
				SameSite = sameSite,
				Path = "/",
				IsEssential = true,
				Expires = DateTime.UtcNow.AddHours(7)
			});

		response.Cookies.Append(
			"refreshToken",
			refreshToken,
			new CookieOptions
			{
				HttpOnly = true,
				Secure = secure,
				SameSite = sameSite,
				Path = "/",
				IsEssential = true,
				Expires = DateTime.UtcNow.AddDays(7)
			});
	}
	public static void DeleteTokenCookies(HttpResponse response, IWebHostEnvironment env)
	{
		var isDev = env.IsDevelopment();

		response.Cookies.Delete("accessToken", new CookieOptions
		{
			HttpOnly = true,
			Secure = !isDev,
			SameSite = isDev ? SameSiteMode.None : SameSiteMode.None,
			Path = "/",
			Expires = DateTimeOffset.UtcNow.AddDays(-1)
		});

		response.Cookies.Delete("refreshToken", new CookieOptions
		{
			HttpOnly = true,
			Secure = !isDev,
			SameSite = isDev ? SameSiteMode.None : SameSiteMode.None,
			Path = "/",
			Expires = DateTimeOffset.UtcNow.AddDays(-1)
		});
	}


	public static string? GetAccessToken(HttpRequest request)
		=> request.Cookies[AccessTokenCookieName];

	public static string? GetRefreshToken(HttpRequest request)
		=> request.Cookies[RefreshTokenCookieName];
}


//	public static void SetTokenCookies(
//		HttpResponse response,
//		string accessToken,
//		string refreshToken,
//		IWebHostEnvironment env)
//	{
//		var isDevelopment = env.IsDevelopment();

//		var baseOptions = new CookieOptions
//		{
//			HttpOnly = true,
//			Secure = !isDevelopment,          // HTTPS only u prod
//			SameSite = SameSiteMode.None,     // OBAVEZNO za React + API
//			Path = "/",
//			IsEssential = true
//		};

//		response.Cookies.Append(
//			AccessTokenCookieName,
//			accessToken,
//			new CookieOptions
//			{
//				HttpOnly = baseOptions.HttpOnly,
//				Secure = baseOptions.Secure,
//				SameSite = baseOptions.SameSite,
//				Path = baseOptions.Path,
//				IsEssential = baseOptions.IsEssential,
//				Expires = DateTime.UtcNow.AddMinutes(30)
//			});

//		response.Cookies.Append(
//			RefreshTokenCookieName,
//			refreshToken,
//			new CookieOptions
//			{
//				HttpOnly = baseOptions.HttpOnly,
//				Secure = baseOptions.Secure,
//				SameSite = baseOptions.SameSite,
//				Path = baseOptions.Path,
//				IsEssential = baseOptions.IsEssential,
//				Expires = DateTime.UtcNow.AddDays(7)
//			});
//	}

//	public static void DeleteTokenCookies(HttpResponse response, IWebHostEnvironment env)
//	{
//		var isDevelopment = env.IsDevelopment();

//		var deleteOptions = new CookieOptions
//		{
//			HttpOnly = true,
//			Secure = false,//!isDevelopment,
//			SameSite = SameSiteMode.Lax, //none prod
//			Path = "/",
//			Expires = DateTimeOffset.UtcNow.AddDays(-1)
//		};

//		response.Cookies.Delete(AccessTokenCookieName, deleteOptions);
//		response.Cookies.Delete(RefreshTokenCookieName, deleteOptions);
//	}

//	public static string? GetAccessToken(HttpRequest request)
//		=> request.Cookies[AccessTokenCookieName];

//	public static string? GetRefreshToken(HttpRequest request)
//		=> request.Cookies[RefreshTokenCookieName];
//}
