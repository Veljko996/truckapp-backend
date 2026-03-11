namespace WebApplication1.Utils.DTOs.NalogTroskoviDTO;

public class NalogTrosakDto
{
    public int TrosakId { get; set; }
    public int NalogId { get; set; }
    public int TipTroskaId { get; set; }
    public string? TipNaziv { get; set; }
    public decimal Iznos { get; set; }
    public string Valuta { get; set; } = string.Empty;
    public string? Napomena { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
