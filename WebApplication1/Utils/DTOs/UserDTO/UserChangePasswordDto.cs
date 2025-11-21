using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Utils.DTOs.UserDTO;

public class UserChangePasswordDto
{
    [Required(ErrorMessage = "Trenutna lozinka je obavezna")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nova lozinka je obavezna")]
    [MinLength(6, ErrorMessage = "Nova lozinka mora imati najmanje 6 karaktera")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Potvrda nove lozinke je obavezna")]
    [Compare("NewPassword", ErrorMessage = "Nova lozinka i potvrda se ne poklapaju")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

