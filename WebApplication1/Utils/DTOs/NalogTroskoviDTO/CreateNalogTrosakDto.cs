using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Utils.DTOs.NalogTroskoviDTO;

public class CreateNalogTrosakDto
{
    [Required]
    public int TipTroskaId { get; set; }

    [Required]
    public decimal Iznos { get; set; }

    [MaxLength(500)]
    public string? Napomena { get; set; }
}
