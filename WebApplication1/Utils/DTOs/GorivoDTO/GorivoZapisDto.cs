namespace WebApplication1.Utils.DTOs.GorivoDTO;

public class GorivoZapisDto
{
    public int GorivoZapisId { get; set; }
    public int VoziloId { get; set; }
    public string? VoziloNaziv { get; set; }
    public int? NalogId { get; set; }
    public string? NalogBroj { get; set; }
    public decimal Iznos { get; set; }
    public string Valuta { get; set; } = string.Empty;
    public decimal KolicineLitara { get; set; }
    public int? Kilometraza { get; set; }
    public DateTime DatumTocenja { get; set; }
    public string? Napomena { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
