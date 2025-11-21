using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Utils.DTOs.PoslovnicaDTO;

public class PoslovnicaCreateDto
{
    [Required(ErrorMessage = "Naziv poslovne jedinice je obavezan")]
    [MaxLength(200, ErrorMessage = "Naziv ne može biti duži od 200 karaktera")]
    public string PJ { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lokacija je obavezna")]
    [MaxLength(200, ErrorMessage = "Lokacija ne može biti duža od 200 karaktera")]
    public string Lokacija { get; set; } = string.Empty;

    [MaxLength(50, ErrorMessage = "Broj telefona ne može biti duži od 50 karaktera")]
    public string? BrojTelefona { get; set; }

    [EmailAddress(ErrorMessage = "Email adresa nije validna")]
    [MaxLength(200, ErrorMessage = "Email ne može biti duži od 200 karaktera")]
    public string? Email { get; set; }
}

