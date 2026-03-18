using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Utils.DTOs.GorivoDTO;

public class CreateGorivoZapisDto
{
    public int? NalogId { get; set; }

    [Range(0, 9_999_999.99, ErrorMessage = "Iznos mora biti između 0 i 9,999,999.99.")]
    public decimal Iznos { get; set; }

    [Required]
    [MaxLength(10)]
    public string Valuta { get; set; } = "RSD";

    [Range(0.01, 99_999.99, ErrorMessage = "Količina litara mora biti veća od 0.")]
    public decimal KolicineLitara { get; set; }

    [Range(0, 9_999_999, ErrorMessage = "Kilometraža mora biti između 0 i 9,999,999.")]
    public int? Kilometraza { get; set; }

    public DateTime DatumTocenja { get; set; }

    [MaxLength(500)]
    public string? Napomena { get; set; }
}
