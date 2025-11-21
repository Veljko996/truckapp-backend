namespace WebApplication1.Utils.DTOs.DashboardDTO;

public class DashboardOverviewDto
{
    public KPIDto Kpi { get; set; } = new();
    public List<StatusDistribucijaDto> StatusDistribucija { get; set; } = new();
    public List<Prihod30DanaDto> Prihod30Dana { get; set; } = new();
    public List<TopTuraDto> TopTure { get; set; } = new();
    public List<KriticnoVoziloDto> KriticnaVozila { get; set; } = new();
    public List<LogDto> Logovi { get; set; } = new();
}

public class KPIDto
{
    public int AktivneTure { get; set; }
    public decimal DanasnjiPrihod { get; set; }
    public int BrojAktivnihVozila { get; set; }
    public int ProblematickeTure { get; set; }
    public int VozilaSaIsticucimDokumentima { get; set; }
}

public class StatusDistribucijaDto
{
    public string Status { get; set; } = string.Empty;
    public int Broj { get; set; }
}

public class Prihod30DanaDto
{
    public DateTime Datum { get; set; }
    public decimal Suma { get; set; }
}

public class TopTuraDto
{
    public int TuraId { get; set; }
    public string RedniBroj { get; set; } = string.Empty;
    public string Relacija { get; set; } = string.Empty;
    public decimal UlaznaCena { get; set; }
    public string PrevoznikNaziv { get; set; } = string.Empty;
    public string? VoziloNaziv { get; set; }
}

public class KriticnoVoziloDto
{
    public int VoziloId { get; set; }
    public string Naziv { get; set; } = string.Empty;
    public string TipProblema { get; set; } = string.Empty; // "Registracija", "Tehnički", "Vinjeta", "PP Aparat"
    public DateTime? DatumIsteka { get; set; }
    public int DanaDoIsteka { get; set; }
}

public class LogDto
{
    public int LogId { get; set; }
    public DateTime HappenedAtDate { get; set; }
    public string? Process { get; set; }
    public string? Activity { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? UserName { get; set; }
}

