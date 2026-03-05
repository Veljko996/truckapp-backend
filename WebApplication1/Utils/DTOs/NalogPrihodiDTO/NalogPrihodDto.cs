namespace WebApplication1.Utils.DTOs.NalogPrihodiDTO;

public class NalogPrihodDto
{
    public int PrihodId { get; set; }
    public int NalogId { get; set; }
    public string TipPrihoda { get; set; } = string.Empty;
    public decimal Iznos { get; set; }
    public string Valuta { get; set; } = string.Empty;
    public string? Napomena { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
