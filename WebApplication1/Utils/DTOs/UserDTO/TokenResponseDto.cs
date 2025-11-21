namespace WebApplication1.Utils.DTOs.UserDTO;

public class TokenResponseDto
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}
