namespace WebApplication1.Utils.DTOs.PrevoznikDTO;

public class PrevoznikDto
{
    public int PrevoznikId { get; set; }
    public string Naziv { get; set; } = string.Empty;
    public string? Kontakt { get; set; }
    public string? Telefon { get; set; }
    public string? PIB { get; set; }
}