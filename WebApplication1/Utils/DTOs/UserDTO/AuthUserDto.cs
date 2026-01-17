namespace WebApplication1.Utils.DTOs.UserDTO;

/// <summary>
/// Lightweight user DTO returned after authentication (login)
/// </summary>
public class AuthUserDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
