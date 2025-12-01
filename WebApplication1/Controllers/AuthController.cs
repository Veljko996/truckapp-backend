using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.AuthenticationServices;
using WebApplication1.Utils;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
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

    [HttpPost("create-admin")]
    public async Task<IActionResult> CreateAdmin()
    {
        var dto = new RegisterUserDto
        {
            Username = "Stefan",
            FullName = "Administrator",
            Email = "stefan@example.com",
            Phone = "000000000",
            RoleId = 1,
            Password = "Admin1!"
        };

        var user = await _service.RegisterAsync(dto);
        return Ok(user);
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

        var isDev = _env.IsDevelopment();
        CookieHelper.SetTokenCookies(Response, result.AccessToken, result.RefreshToken, isDev);

        return Ok(new { message = "Login successful" });
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult> RefreshToken()
    {
        var accessToken = Request.Cookies["accessToken"];
        var refreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrWhiteSpace(refreshToken))
            return BadRequest(new { message = "Refresh token je obavezan." });

        var result = await _service.RefreshTokensAsync(new RefreshTokenRequestDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });

        if (result is null)
            return Unauthorized(new { message = "Invalid refresh token." });

        CookieHelper.SetTokenCookies(Response, result.AccessToken, result.RefreshToken, _env.IsDevelopment());

        return Ok(new { message = "Token refreshed successfully" });
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        try
        {
            _service.LogoutAsync();

            CookieHelper.DeleteTokenCookies(Response, _env.IsDevelopment());

            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri logout-u korisnika");

            CookieHelper.DeleteTokenCookies(Response, _env.IsDevelopment());

            return Ok(new { message = "Logged out successfully" });
        }
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
