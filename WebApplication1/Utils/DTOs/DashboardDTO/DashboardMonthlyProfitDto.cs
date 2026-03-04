namespace WebApplication1.Utils.DTOs.DashboardDTO;

public class DashboardMonthlyProfitDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal ProfitEUR { get; set; }
    public decimal ProfitRSD { get; set; }
}
