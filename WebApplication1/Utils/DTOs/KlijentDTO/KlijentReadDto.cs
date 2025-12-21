namespace WebApplication1.Utils.DTOs.KlijentDTO;

public class KlijentReadDto
{
    public int KlijentId { get; set; }
    public string NazivFirme { get; set; } = string.Empty;
    public string Drzava { get; set; } = string.Empty;
    public string? Grad { get; set; }
    public string? Adresa { get; set; }
    public string? PIB { get; set; }
    public string? PoreskiBroj { get; set; }
    public string? KontaktOsoba { get; set; }
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? Napomena { get; set; }
    public bool Aktivan { get; set; }
    public DateTime DatumKreiranja { get; set; }
    public DateTime? DatumAzuriranja { get; set; }
}

