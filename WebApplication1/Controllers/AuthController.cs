using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.AuthenticationServices;
using WebApplication1.Utils;


namespace WebApplication1.Controllers;
[ApiController]
[Route("api/[controller]")]

public class AuthController: ControllerBase
{
    private readonly IAuthService _service;
    private readonly ILogger<AuthController> _logger;
    private readonly IWebHostEnvironment _env;

    public AuthController(IAuthService service, ILogger<AuthController> logger, IWebHostEnvironment env)
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
    public async Task<ActionResult> Login(LoginUserDto request)
    {
        var result = await _service.LoginAsync(request);
        if (result is null)
            return BadRequest("Invalid username or password.");

        // Set HTTP-only cookies instead of returning tokens in response body
        var isDevelopment = _env.IsDevelopment();
        CookieHelper.SetTokenCookies(Response, result.AccessToken, result.RefreshToken, isDevelopment);

        return Ok(new { message = "Login successful" });
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult> RefreshToken(RefreshTokenRequestDto? request = null)
    {
        var result = await _service.RefreshTokensAsync(request);
        if (result is null || result.AccessToken is null || result.RefreshToken is null)
            return Unauthorized("Invalid refresh token.");

        // Set new HTTP-only cookies
        var isDevelopment = _env.IsDevelopment();
        CookieHelper.SetTokenCookies(Response, result.AccessToken, result.RefreshToken, isDevelopment);

        return Ok(new { message = "Token refreshed successfully" });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout()
    {
        try
        {
            await _service.LogoutAsync();
            
            // Delete cookies
            var isDevelopment = _env.IsDevelopment();
            CookieHelper.DeleteTokenCookies(Response, isDevelopment);

            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri logout-u korisnika");
            // Still delete cookies even if logout fails
            var isDevelopment = _env.IsDevelopment();
            CookieHelper.DeleteTokenCookies(Response, isDevelopment);
            return Ok(new { message = "Logged out successfully" });
        }
    }

    [HttpGet("me")]
    [Authorize] 
    public IActionResult Me()
    {
        // Izdvaja podatke direktno iz tokena
        var username = User.Identity?.Name ?? "unknown";
        var id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        return Ok(new
        {
            UserId = id,
            Username = username,
            Role = role
        });
    }

}
