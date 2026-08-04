using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.ReportRepository;

/// <summary>Sirovi podaci za izveštaj o krugovima (batch, bez N+1). Tenant filter dolazi automatski iz EF global filtera.</summary>
public class KrugReportData
{
    public List<Krug> Krugovi { get; set; } = new();
    public List<Nalog> Nalozi { get; set; } = new();
    public List<GorivoZapis> Gorivo { get; set; } = new();
}

public interface IReportRepository
{
    Task<KrugReportData> GetKrugReportDataAsync(DateTime from, DateTime to, int? voziloId);
}
