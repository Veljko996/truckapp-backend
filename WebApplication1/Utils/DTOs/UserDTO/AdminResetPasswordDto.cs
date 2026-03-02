using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Utils.DTOs.UserDTO;

public class AdminResetPasswordDto
{
    [Required]
    public string NewPassword { get; set; } = string.Empty;
}
