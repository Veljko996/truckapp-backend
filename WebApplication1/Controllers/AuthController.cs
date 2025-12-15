using WebApplication1.Services.AuthenticationServices;
using WebApplication1.Utils;


namespace WebApplication1.Controllers;
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]

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

    //[HttpPost("create-admin")]
    //public async Task<IActionResult> CreateAdmin()
    //{
    //    var dto = new RegisterUserDto
    //    {
    //        Username = "Stefan",
    //        FullName = "Administrator",
    //        Email = "stefan@example.com",
    //        Phone = "000000000",
    //        RoleId = 1,
    //        Password = "Admin1!"
    //    };

    //    var user = await _service.RegisterAsync(dto);
    //    return Ok(user);
    //}

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
public async Task<ActionResult> RefreshToken()
{
    try
    {
        var accessToken = Request.Cookies["accessToken"];
        var refreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrWhiteSpace(refreshToken))
            return BadRequest(new { status = 400, message = "Refresh token je obavezan." });

        var result = await _service.RefreshTokensAsync(new RefreshTokenRequestDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });

        if (result is null)
            return Unauthorized(new { status = 401, message = "Invalid refresh token." });

        var isDevelopment = _env.IsDevelopment();
        CookieHelper.SetTokenCookies(Response, result.AccessToken, result.RefreshToken, isDevelopment);

        return Ok(new { message = "Token refreshed successfully" });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Greška pri refresh token operaciji");
        // Exception će biti obrađen od strane ErrorHandlerMiddleware
        throw;
    }
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
