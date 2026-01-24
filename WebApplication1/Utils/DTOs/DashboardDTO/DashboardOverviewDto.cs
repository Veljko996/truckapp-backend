namespace WebApplication1.Utils.DTOs.DashboardDTO;

/// <summary>
/// Main dashboard overview DTO containing all dashboard metrics.
/// </summary>
public class DashboardOverviewDto
{
    public KPIDto Kpi { get; set; } = new();
    public List<TopTuraDto> TopTure { get; set; } = new();
    public List<KriticnoVoziloDto> KriticnaVozila { get; set; } = new();
    public List<TopCarrierDto> TopCarriers { get; set; } = new();
    public List<TopClientDto> TopClients { get; set; } = new();
    public List<LateUnloadNalogDto> LateUnloadNalogs { get; set; } = new();
}

/// <summary>
/// Key Performance Indicators for dashboard.
/// </summary>
public class KPIDto
{
    public int UkupnoTura { get; set; }
    public int AktivneTure { get; set; }
    public int TotalNalogsCount { get; set; }
    public int AktivniNalozi { get; set; }
    public int UkupnoVozila { get; set; }
    public int AktivnaVozila { get; set; }
    public int VozilaSaIsticucimDokumentima { get; set; }
    public int AktivniKlijenti { get; set; }
    public int AktivniPrevoznici { get; set; }
    public int ActiveNalogsCount { get; set; }
    public int LateUnloadNalogsCount { get; set; }
    public decimal ProfitLast30DaysEUR { get; set; }
    public decimal ProfitLast30DaysRSD { get; set; }
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
/// Late unload Nalog DTO.
/// </summary>
public class LateUnloadNalogDto
{
    public int NalogId { get; set; }
    public string NalogBroj { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public DateTime? PlannedUnloadDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Top tours by profit DTO.
/// </summary>
public class TopTuraDto
{
    public int TuraId { get; set; }
    public string RedniBroj { get; set; } = string.Empty;
    public string Relacija { get; set; } = string.Empty;
    public decimal? ProfitEUR { get; set; }
    public decimal? ProfitRSD { get; set; }
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
/// Top 5 carriers by tour count (last 30 days).
/// </summary>
public class TopCarrierDto
{
    public int CarrierId { get; set; }
    public string CarrierName { get; set; } = string.Empty;
    public int TotalToursCount { get; set; }
}

/// <summary>
/// Top 5 clients by profit (last 30 days).
/// </summary>
public class TopClientDto
{
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public decimal TotalProfitEUR { get; set; }
    public decimal TotalProfitRSD { get; set; }
}
