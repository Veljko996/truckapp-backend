namespace WebApplication1.Utils.DTOs.DashboardDTO;

/// <summary>
/// Raw result shape from dbo.GetExternalDashboardStats.
/// Nullable so that NULL from SQL maps without throwing; repository normalizes to 0 when building DashboardStatsDto.
/// </summary>
public class ExternalDashboardStatsRawDto
{
    public int? UkupanBrojNaloga { get; set; }
    public int? UkupanBrojIzvrsenihNaloga { get; set; }
    public int? BrojAktivnihNaloga { get; set; }
    public int? BrojNalogaDanas { get; set; }
    public int? BrojNalogaNedelja { get; set; }
    public int? BrojNalogaMesec { get; set; }
    public decimal? ProfitDanasEUR { get; set; }
    public decimal? ProfitDanasRSD { get; set; }
    public decimal? ProfitNedeljaEUR { get; set; }
    public decimal? ProfitNedeljaRSD { get; set; }
    public decimal? ProfitMesecEUR { get; set; }
    public decimal? ProfitMesecRSD { get; set; }
}
