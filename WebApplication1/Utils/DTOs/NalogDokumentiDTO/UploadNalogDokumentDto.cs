using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Utils.DTOs.NalogDokumentiDTO;

public class UploadNalogDokumentDto
{
    [Required]
    public IFormFile File { get; set; } = null!;

    [Required]
    public int TipDokumentaId { get; set; }

    [MaxLength(500)]
    public string? Napomena { get; set; }
}
