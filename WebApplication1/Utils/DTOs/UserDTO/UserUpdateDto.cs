using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Utils.DTOs.UserDTO;

public class UserUpdateDto
{
    [Required(ErrorMessage = "ID korisnika je obavezan")]
    public int UserId { get; set; }

    [MaxLength(200, ErrorMessage = "Ime i prezime ne može biti duže od 200 karaktera")]
    public string? FullName { get; set; }

    [EmailAddress(ErrorMessage = "Email adresa nije validna")]
    [MaxLength(200, ErrorMessage = "Email ne može biti duži od 200 karaktera")]
    public string? Email { get; set; }

    [MaxLength(30, ErrorMessage = "Telefon ne može biti duži od 30 karaktera")]
    [Phone(ErrorMessage = "Broj telefona nije validan")]
    public string? Phone { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Uloga mora biti validna")]
    public int? RoleId { get; set; }

    public bool? IsActive { get; set; }
}

