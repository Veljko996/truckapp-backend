using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.DashboardRepository;

public interface IDashboardRepository
{
    // KPI
    Task<int> GetAktivneTureCountAsync();
    Task<decimal> GetDanasnjiPrihodAsync();
    Task<int> GetAktivnaVozilaCountAsync();
    Task<int> GetProblematickeTureCountAsync();
    Task<int> GetVozilaSaIsticucimDokumentimaCountAsync(int daysThreshold = 7);
    
    // Status distribucija
    Task<List<(string Status, int Count)>> GetStatusDistribucijaAsync();
    
    // Prihod 30 dana
    Task<List<(DateTime Datum, decimal Suma)>> GetPrihod30DanaAsync();
    
    // Top ture
    Task<List<Tura>> GetTopTureAsync(int topCount = 5);
    
    // Kritična vozila
    Task<List<NasaVozila>> GetKriticnaVozilaAsync(int daysThreshold = 7);
    
    // Logovi
    Task<List<Log>> GetNajnovijiLogoviAsync(int count = 10);
}

