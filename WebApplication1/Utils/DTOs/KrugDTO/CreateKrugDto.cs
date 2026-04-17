using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Utils.DTOs.KrugDTO;

public class CreateKrugDto
{
    [Required]
    public int VoziloId { get; set; }

    public DateTime? StartAt { get; set; }

    [MaxLength(500)]
    public string? Napomena { get; set; }
}
