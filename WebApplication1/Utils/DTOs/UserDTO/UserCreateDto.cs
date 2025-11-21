using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Utils.DTOs.UserDTO;

public class UserCreateDto
{
    [Required(ErrorMessage = "Korisničko ime je obavezno")]
    [MaxLength(50, ErrorMessage = "Korisničko ime ne može biti duže od 50 karaktera")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lozinka je obavezna")]
    [MinLength(6, ErrorMessage = "Lozinka mora imati najmanje 6 karaktera")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ime i prezime su obavezni")]
    [MaxLength(200, ErrorMessage = "Ime i prezime ne može biti duže od 200 karaktera")]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Email adresa nije validna")]
    [MaxLength(200, ErrorMessage = "Email ne može biti duži od 200 karaktera")]
    public string? Email { get; set; }

    [MaxLength(30, ErrorMessage = "Telefon ne može biti duži od 30 karaktera")]
    [Phone(ErrorMessage = "Broj telefona nije validan")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Uloga je obavezna")]
    [Range(1, int.MaxValue, ErrorMessage = "Uloga mora biti validna")]
    public int RoleId { get; set; }

    public bool IsActive { get; set; } = true;
}

