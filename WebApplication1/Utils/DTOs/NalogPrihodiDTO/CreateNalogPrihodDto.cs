using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Utils.DTOs.NalogPrihodiDTO;

public class CreateNalogPrihodDto
{
    [Required, MaxLength(100)]
    public string TipPrihoda { get; set; } = string.Empty;

    [Required]
    public decimal Iznos { get; set; }

    [Required, MaxLength(10)]
    public string Valuta { get; set; } = "RSD";

    [MaxLength(500)]
    public string? Napomena { get; set; }
}
