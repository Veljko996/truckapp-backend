namespace WebApplication1.Utils.DTOs.DashboardDTO;

/// <summary>
/// Main dashboard overview DTO containing all dashboard metrics.
/// </summary>
public class DashboardOverviewDto
{
    public KPIDto Kpi { get; set; } = new();
    public List<StatusDistribucijaDto> TureStatusDistribucija { get; set; } = new();
    public List<StatusDistribucijaDto> NaloziStatusDistribucija { get; set; } = new();
    public List<Prihod30DanaDto> Prihod30Dana { get; set; } = new();
    public List<TopTuraDto> TopTure { get; set; } = new();
    public List<KriticnoVoziloDto> KriticnaVozila { get; set; } = new();
    public List<LogDto> NajnovijiLogovi { get; set; } = new();
}

/// <summary>
/// Key Performance Indicators for dashboard.
/// </summary>
public class KPIDto
{
    public int UkupnoTura { get; set; }
    public int AktivneTure { get; set; }
    public int UkupnoNalozi { get; set; }
    public int AktivniNalozi { get; set; }
    public decimal DanasnjiPrihod { get; set; }
    public decimal Prihod30Dana { get; set; }
    public int UkupnoVozila { get; set; }
    public int AktivnaVozila { get; set; }
    public int VozilaSaIsticucimDokumentima { get; set; }
    public int AktivniKlijenti { get; set; }
    public int AktivniPrevoznici { get; set; }
}

/// <summary>
/// Status distribution DTO.
/// </summary>
public class StatusDistribucijaDto
{
    public string Status { get; set; } = string.Empty;
    public int Broj { get; set; }
}

/// <summary>
/// Revenue data for a specific date.
/// </summary>
public class Prihod30DanaDto
{
    public DateTime Datum { get; set; }
    public decimal Suma { get; set; }
}

/// <summary>
/// Top tours by price DTO.
/// </summary>
public class TopTuraDto
{
    public int TuraId { get; set; }
    public string RedniBroj { get; set; } = string.Empty;
    public string Relacija { get; set; } = string.Empty;
    public decimal UlaznaCena { get; set; }
    public string? PrevoznikNaziv { get; set; }
    public string? VoziloNaziv { get; set; }
    public string? KlijentNaziv { get; set; }
    public string StatusTure { get; set; } = string.Empty;
}

/// <summary>
/// Critical vehicle with expiring documents DTO.
/// </summary>
public class KriticnoVoziloDto
{
    public int VoziloId { get; set; }
    public string Naziv { get; set; } = string.Empty;
    public string TipProblema { get; set; } = string.Empty; // "Registracija", "Tehnički", "Vinjeta", "PP Aparat"
    public DateTime? DatumIsteka { get; set; }
    public int DanaDoIsteka { get; set; }
    public string? Detalji { get; set; } // Za vinjete: država kod
}

/// <summary>
/// Log entry DTO for recent activities.
/// </summary>
public class LogDto
{
    public int LogId { get; set; }
    public DateTime HappenedAtDate { get; set; }
    public string? Process { get; set; }
    public string? Activity { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? UserName { get; set; }
}
