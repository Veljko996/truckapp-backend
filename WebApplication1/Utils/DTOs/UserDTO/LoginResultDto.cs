namespace WebApplication1.Utils.DTOs.UserDTO;

/// <summary>
/// Result returned from login operation - contains tokens and user data
/// </summary>
public class LoginResultDto
{
    public required TokenResponseDto Tokens { get; set; }
    public required AuthUserDto User { get; set; }
}
