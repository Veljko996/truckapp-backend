using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication1.Services.AuthenticationServices;
using WebApplication1.Services.UserServices;
using WebApplication1.Utils.DTOs.UserDTO;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, IAuthService authService, ILogger<UserController> logger)
    {
        _userService = userService;
        _authService = authService;
        _logger = logger;
    }

    // ==================== USER ENDPOINTS ====================

    /// <summary>
    /// Vraća sve korisnike
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserReadDto>>> GetAllUsers([FromQuery] bool includeInactive = false)
    {
        try
        {
            var users = await _userService.GetAllUsersAsync(includeInactive);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri dohvatanju korisnika");
            throw;
        }
    }

    /// <summary>
    /// Vraća korisnika po ID-u
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserReadDto>> GetUserById(int id, [FromQuery] bool includeEmployee = false)
    {
        try
        {
            UserReadDto? user;
            
            if (includeEmployee)
                user = await _userService.GetUserByIdWithEmployeeAsync(id);
            else
                user = await _userService.GetUserByIdAsync(id);

            if (user is null)
                return NotFound(new { message = $"Korisnik sa ID {id} nije pronađen." });

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri dohvatanju korisnika ID: {UserId}", id);
            throw;
        }
    }

    /// <summary>
    /// Vraća korisnike po ulozi
    /// </summary>
    [HttpGet("role/{roleId}")]
    public async Task<ActionResult<IEnumerable<UserReadDto>>> GetUsersByRole(int roleId)
    {
        try
        {
            var users = await _userService.GetUsersByRoleIdAsync(roleId);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri dohvatanju korisnika po ulozi: {RoleId}", roleId);
            throw;
        }
    }

    /// <summary>
    /// Kreira novog korisnika
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")] // Samo admin može kreirati korisnike
    public async Task<ActionResult<UserReadDto>> CreateUser([FromBody] UserCreateDto createDto)
    {
        try
        {
            var user = await _userService.CreateUserAsync(createDto);
            return CreatedAtAction(nameof(GetUserById), new { id = user.UserId }, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri kreiranju korisnika: {Username}", createDto.Username);
            throw;
        }
    }

    /// <summary>
    /// Ažurira korisnika
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto updateDto)
    {
        try
        {
            // Provera da li korisnik pokušava da ažurira sebe ili je admin
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && currentUserId != id)
                return Forbid("Možete ažurirati samo svoj profil.");

            // Non-admin users may only edit basic profile fields on themselves.
            if (!isAdmin && (updateDto.RoleId.HasValue || updateDto.IsActive.HasValue))
                return Forbid("Nemate dozvolu za izmenu uloge ili statusa korisnika.");

            updateDto.UserId = id;
            await _userService.UpdateUserAsync(updateDto, isAdmin);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri ažuriranju korisnika ID: {UserId}", id);
            throw;
        }
    }

    /// <summary>
    /// Briše korisnika (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        try
        {
            var deleted = await _userService.DeleteUserAsync(id);
            if (!deleted)
                return BadRequest(new { message = "Korisnik nije obrisan." });

            return Ok(new { message = "Korisnik je uspešno obrisan." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri brisanju korisnika ID: {UserId}", id);
            throw;
        }
    }

    /// <summary>
    /// Menja lozinku korisnika
    /// </summary>
    [HttpPost("{id}/change-password")]
    public async Task<ActionResult> ChangePassword(int id, [FromBody] UserChangePasswordDto changePasswordDto)
    {
        try
        {
            // Provera da li korisnik pokušava da promeni svoju lozinku
            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && currentUserId != id)
                return Forbid("Možete promeniti samo svoju lozinku.");

            var changed = await _userService.ChangePasswordAsync(id, changePasswordDto);
            if (!changed)
                return BadRequest(new { message = "Lozinka nije promenjena." });

            return Ok(new { message = "Lozinka je uspešno promenjena." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri promeni lozinke korisnika ID: {UserId}", id);
            throw;
        }
    }

    /// <summary>
    /// Aktivira korisnika
    /// </summary>
    [HttpPost("{id}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> ActivateUser(int id)
    {
        try
        {
            var activated = await _userService.ActivateUserAsync(id);
            if (!activated)
                return BadRequest(new { message = "Korisnik nije aktiviran." });

            return Ok(new { message = "Korisnik je uspešno aktiviran." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri aktivaciji korisnika ID: {UserId}", id);
            throw;
        }
    }

    /// <summary>
    /// Deaktivira korisnika
    /// </summary>
    [HttpPost("{id}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeactivateUser(int id)
    {
        try
        {
            var deactivated = await _userService.DeactivateUserAsync(id);
            if (!deactivated)
                return BadRequest(new { message = "Korisnik nije deaktiviran." });

            return Ok(new { message = "Korisnik je uspešno deaktiviran." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri deaktivaciji korisnika ID: {UserId}", id);
            throw;
        }
    }

    [HttpPost("/api/admin/users/{userId:int}/reset-password")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ResetPasswordAsAdmin(int userId, [FromBody] AdminResetPasswordDto request)
    {
        await _authService.AdminResetPasswordAsync(userId, request);
        return Ok(new { message = "Lozinka je uspešno resetovana." });
    }

    // ==================== EMPLOYEE ENDPOINTS ====================
}

