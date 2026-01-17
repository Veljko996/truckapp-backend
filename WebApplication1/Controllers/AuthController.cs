using WebApplication1.Services.AuthenticationServices;
using WebApplication1.Utils;
using WebApplication1.Utils.DTOs.UserDTO;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
	private readonly IAuthService _service;
	private readonly ILogger<AuthController> _logger;
	private readonly IWebHostEnvironment _env;

	public AuthController(
		IAuthService service,
		ILogger<AuthController> logger,
		IWebHostEnvironment env)
	{
		_service = service;
		_logger = logger;
		_env = env;
	}

	[HttpPost("register")]
	public async Task<ActionResult<User>> Register(RegisterUserDto request)
	{
		try
		{
			var user = await _service.RegisterAsync(request);
			if (user is null)
				return BadRequest("Username already exists.");

			return Ok(user);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Greška u registraciji korisnika: {Username}", request.Username);
			throw;
		}
	}

	[HttpPost("login")]
	public async Task<IActionResult> Login(LoginUserDto request)
	{
		var result = await _service.LoginAsync(request);
		if (result is null)
			return BadRequest("Invalid username or password.");

		// Set tokens in HTTP-only cookies
		CookieHelper.SetTokenCookies(
			Response,
			result.Tokens.AccessToken,
			result.Tokens.RefreshToken,
			_env);

		// Return user data in response body (tokens are in cookies only)
		return Ok(new { user = result.User });
	}

	[HttpPost("refresh-token")]
	public async Task<IActionResult> RefreshToken()
	{
		try
		{
			var refreshToken = CookieHelper.GetRefreshToken(Request);
			var accessToken = CookieHelper.GetAccessToken(Request);

			if (string.IsNullOrWhiteSpace(refreshToken))
				return BadRequest(new { message = "Refresh token is required." });

			var result = await _service.RefreshTokensAsync(new RefreshTokenRequestDto
			{
				AccessToken = accessToken,
				RefreshToken = refreshToken
			});

			if (result is null)
				return Unauthorized(new { message = "Invalid refresh token." });

			CookieHelper.SetTokenCookies(
				Response,
				result.AccessToken,
				result.RefreshToken,
				_env);

			return Ok(new { message = "Token refreshed successfully" });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Greška pri refresh token operaciji");
			throw;
		}
	}

	[HttpPost("logout")]
	[Authorize]
	public async Task<IActionResult> Logout()
	{
		try
		{
			await _service.LogoutAsync();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Greška pri logout-u korisnika");
		}
		finally
		{
			// Cookie se briše BEZ OBZIRA
			CookieHelper.DeleteTokenCookies(Response, _env);
		}

		return Ok(new { message = "Logged out successfully" });
	}

	[HttpGet("me")]
	[Authorize]
	public IActionResult Me()
	{
		return Ok(new
		{
			UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
			Username = User.Identity?.Name,
			Role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
		});
	}
}
