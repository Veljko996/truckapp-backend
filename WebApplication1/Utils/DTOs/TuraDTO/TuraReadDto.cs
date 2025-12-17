namespace WebApplication1.Utils.DTOs.TuraDTO;

public class TuraReadDto
{
    public int TuraId { get; set; }
    public string RedniBroj { get; set; } = string.Empty;

    public string MestoUtovara { get; set; } = string.Empty;
    public string MestoIstovara { get; set; } = string.Empty;

    public DateTime? DatumUtovaraOd { get; set; }
    public DateTime? DatumUtovaraDo { get; set; }
    public DateTime? DatumIstovaraOd { get; set; }
    public DateTime? DatumIstovaraDo { get; set; }

    public string? KolicinaRobe { get; set; }
    public string? Tezina { get; set; }

    public string OpcijaPrevoza { get; set; } = string.Empty;

    public int VrstaNadogradnjeId { get; set; }
    public string VrstaNadogradnjeNaziv { get; set; } = string.Empty;

    public string? Napomena { get; set; }
    public string? NapomenaKlijenta { get; set; }

    public decimal? UlaznaCena { get; set; }
    public decimal? IzlaznaCena { get; set; }
    public string Valuta { get; set; } = "EUR";

    public string StatusTure { get; set; } = string.Empty;
    public bool KreiranPutniNalog { get; set; }

    public int KlijentId { get; set; }
    public string KlijentNaziv { get; set; } = string.Empty;

    public int PrevoznikId { get; set; }
    public string PrevoznikNaziv { get; set; } = string.Empty;

    public int? VoziloId { get; set; }
    public string? VoziloNaziv { get; set; }
}
