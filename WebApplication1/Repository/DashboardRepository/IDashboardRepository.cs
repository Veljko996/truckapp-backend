using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.DashboardRepository;

/// <summary>
/// Repository interface for dashboard data aggregation.
/// Contains only data access methods - no business logic.
/// </summary>
public interface IDashboardRepository
{
    // === Ture Statistics ===
    Task<int> GetTotalTureCountAsync();
    Task<int> GetAktivneTureCountAsync();
    Task<Dictionary<string, int>> GetTureStatusDistribucijaAsync();
    Task<List<Tura>> GetTopTureByPriceAsync(int take, CancellationToken cancellationToken = default);
    Task<decimal> GetPrihodZaDanasAsync();
    Task<decimal> GetPrihodZaPeriodAsync(DateTime startDate, DateTime endDate);
    Task<List<(DateTime Datum, decimal Suma)>> GetPrihodByDateAsync(DateTime startDate, DateTime endDate);

    // === Nalozi Statistics ===
    Task<int> GetTotalNaloziCountAsync();
    Task<int> GetAktivniNaloziCountAsync();
    Task<Dictionary<string, int>> GetNaloziStatusDistribucijaAsync();

    // === Vozila Statistics ===
    Task<int> GetTotalVozilaCountAsync();
    Task<int> GetAktivnaVozilaCountAsync();
    Task<List<NasaVozila>> GetVozilaSaIsticucimDokumentimaAsync(int daysThreshold, CancellationToken cancellationToken = default);
    Task<List<NasaVozila>> GetVozilaZaDashboardAsync(CancellationToken cancellationToken = default);

    // === Vinjete Statistics ===
    Task<int> GetIsticeVinjetaCountAsync(int daysThreshold);

    // === Other ===
    Task<List<Log>> GetNajnovijiLogoviAsync(int count, CancellationToken cancellationToken = default);
    Task<int> GetAktivniKlijentiCountAsync();
    Task<int> GetAktivniPrevozniciCountAsync();
}
