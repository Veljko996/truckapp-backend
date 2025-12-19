public class TuraReadDto
{
    public int TuraId { get; set; }
    public string? RedniBroj { get; set; }
    public string MestoUtovara { get; set; } = string.Empty;
    public string MestoIstovara { get; set; } = string.Empty;

    public DateTime? DatumUtovaraOd { get; set; }
    public DateTime? DatumUtovaraDo { get; set; }
    public DateTime? DatumIstovaraOd { get; set; }
    public DateTime? DatumIstovaraDo { get; set; }

    public string? KolicinaRobe { get; set; }
    public string? Tezina { get; set; }

    public decimal? UlaznaCena { get; set; }
    public decimal? IzlaznaCena { get; set; }

    public string? Valuta { get; set; }
    public string? StatusTure { get; set; }

    public int? KlijentId { get; set; }
    public int? PrevoznikId { get; set; }
    public int? VoziloId { get; set; }
    public int VrstaNadogradnjeId { get; set; }

    public bool KreiranPutniNalog { get; set; }

    // sve lookup nazive nullable
    public string? PrevoznikNaziv { get; set; }
    public string? VoziloNaziv { get; set; }
    public string? KlijentNaziv { get; set; }
    public string? VrstaNadogradnjeNaziv { get; set; }
}
