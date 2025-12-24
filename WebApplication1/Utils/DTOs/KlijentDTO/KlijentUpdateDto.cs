using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Utils.DTOs.KlijentDTO;

public class KlijentUpdateDto
{
    [MaxLength(200)]
    public string? NazivFirme { get; set; }

    [MaxLength(100)]
    public string? Drzava { get; set; }

    [MaxLength(100)]
    public string? Grad { get; set; }

    [MaxLength(255)]
    public string? Adresa { get; set; }

    [MaxLength(50)]
    public string? PIB { get; set; }

    [MaxLength(50)]
    public string? PoreskiBroj { get; set; }

    [MaxLength(150)]
    public string? KontaktOsoba { get; set; }

    [MaxLength(50)]
    public string? Telefon { get; set; }

    [MaxLength(150)]
    public string? Email { get; set; }

    [MaxLength(350)]
    public string? Napomena { get; set; }

    public bool? Aktivan { get; set; }
}

