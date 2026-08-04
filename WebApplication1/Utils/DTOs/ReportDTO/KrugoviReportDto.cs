using WebApplication1.Utils.DTOs.NalogPrihodiDTO;

namespace WebApplication1.Utils.DTOs.ReportDTO;

/// <summary>
/// Izveštaj o zatvorenim krugovima u datumskom opsegu (po datumu zatvaranja kruga).
/// Sadrži dva prikaza istih podataka: po krugu i zbirno po vozilu, plus ukupno.
/// Sve novčane vrednosti su po valuti (bez konverzije).
/// </summary>
public class KrugoviReportDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public int BrojKrugova { get; set; }

    public List<KrugReportRowDto> Krugovi { get; set; } = new();
    public List<VoziloReportRowDto> PoVozilu { get; set; } = new();
    public ReportTotalsDto Ukupno { get; set; } = new();
}

public class KrugReportRowDto
{
    public int KrugId { get; set; }
    public string? Broj { get; set; }
    public int VoziloId { get; set; }
    public string? VoziloNaziv { get; set; }
    public DateTime? Zatvoren { get; set; }
    public int? PredjeniKm { get; set; }
    public decimal Litara { get; set; }
    public int BrojNaloga { get; set; }

    public List<AmountByCurrencyDto> Prihod { get; set; } = new();
    /// <summary>Ukupni troškovi = troškovi kruga + gorivo + troškovi naloga (kao u rezimeu kruga).</summary>
    public List<AmountByCurrencyDto> Troskovi { get; set; } = new();
    public List<AmountByCurrencyDto> Profit { get; set; } = new();
}

public class VoziloReportRowDto
{
    public int VoziloId { get; set; }
    public string? VoziloNaziv { get; set; }
    public int BrojKrugova { get; set; }
    public int PredjeniKm { get; set; }
    public decimal Litara { get; set; }

    public List<AmountByCurrencyDto> Prihod { get; set; } = new();
    public List<AmountByCurrencyDto> Troskovi { get; set; } = new();
    public List<AmountByCurrencyDto> Profit { get; set; } = new();
}

public class ReportTotalsDto
{
    public List<AmountByCurrencyDto> Prihod { get; set; } = new();
    public List<AmountByCurrencyDto> Troskovi { get; set; } = new();
    public List<AmountByCurrencyDto> Profit { get; set; } = new();
}
