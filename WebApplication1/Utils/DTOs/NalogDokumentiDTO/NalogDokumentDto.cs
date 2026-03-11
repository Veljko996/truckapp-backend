namespace WebApplication1.Utils.DTOs.NalogDokumentiDTO;

public class NalogDokumentDto
{
    public int DokumentId { get; set; }
    public int NalogId { get; set; }
    public int TipDokumentaId { get; set; }
    public string? TipDokumentaNaziv { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Napomena { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
