namespace WebApplication1.Utils.DTOs.TuraDTO;

public class TuraReadDto
{
    public int TuraId { get; set; }
    public string RedniBroj { get; set; } = string.Empty;
    public string Relacija { get; set; } = string.Empty;

    public DateTime UtovarDatum { get; set; }
    public DateTime IstovarDatum { get; set; }

    public string? KolicinaRobe { get; set; }
    public string? Tezina { get; set; }

    public string OpcijaPrevoza { get; set; } = string.Empty;
    public string? VrstaNadogradnje { get; set; }
    public string? Napomena { get; set; }
    public decimal? UlaznaCena { get; set; }

    public string StatusTrenutni { get; set; } = string.Empty;
    public DateTime StatusTrenutniVreme { get; set; }
    public string StatusKonacni { get; set; } = string.Empty;

    public int PrevoznikId { get; set; }
    public string PrevoznikNaziv { get; set; } = string.Empty;

    public int? VoziloId { get; set; }
    public string? VoziloNaziv { get; set; }
}