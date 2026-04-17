namespace WebApplication1.Utils.DTOs.KrugDTO;

public class KrugReadDto
{
    public int KrugId { get; set; }
    public string? Broj { get; set; }
    public int VoziloId { get; set; }
    public string? VoziloNaziv { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Napomena { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? ClosedBy { get; set; }

    public int BrojTura { get; set; }
    public int BrojNaloga { get; set; }
}
