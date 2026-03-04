namespace WebApplication1.Utils.DTOs.DashboardDTO;

public class DashboardStatsDto
{
    public int UkupanBrojNaloga { get; set; }
    public int UkupanBrojIzvrsenihNaloga { get; set; }
    public int BrojAktivnihNaloga { get; set; }
    public int BrojNalogaDanas { get; set; }
    public int BrojNalogaNedelja { get; set; }
    public int BrojNalogaMesec { get; set; }
    public decimal PrihodDanas { get; set; }
    public decimal PrihodNedelja { get; set; }
    public decimal PrihodMesec { get; set; }
    public decimal ProfitNedeljaEUR { get; set; }
    public decimal ProfitMesecEUR { get; set; }
    public decimal ProfitNedeljaRSD { get; set; }
    public decimal ProfitMesecRSD { get; set; }
}
