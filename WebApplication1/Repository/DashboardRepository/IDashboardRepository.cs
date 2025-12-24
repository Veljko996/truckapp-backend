using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.DashboardRepository;

public interface IDashboardRepository
{
    Task<int> GetTotalTureCountAsync();
    Task<int> GetAktivneTureCountAsync();
    Task<Dictionary<string, int>> GetTureStatusDistribucijaAsync();
    Task<List<Tura>> GetTopTureByPriceAsync(int take, CancellationToken cancellationToken = default);
    Task<decimal> GetPrihodZaDanasAsync();
    Task<decimal> GetPrihodZaPeriodAsync(DateTime startDate, DateTime endDate);
    Task<List<(DateTime Datum, decimal Suma)>> GetPrihodByDateAsync(DateTime startDate, DateTime endDate);

    Task<int> GetTotalNaloziCountAsync();
    Task<int> GetAktivniNaloziCountAsync();
    Task<Dictionary<string, int>> GetNaloziStatusDistribucijaAsync();

    Task<int> GetTotalVozilaCountAsync();
    Task<int> GetAktivnaVozilaCountAsync();
    Task<List<NasaVozila>> GetVozilaSaIsticucimDokumentimaAsync(int daysThreshold, CancellationToken cancellationToken = default);
    Task<List<NasaVozila>> GetVozilaZaDashboardAsync(CancellationToken cancellationToken = default);

    Task<int> GetIsticeVinjetaCountAsync(int daysThreshold);
    Task<List<Log>> GetNajnovijiLogoviAsync(int count, CancellationToken cancellationToken = default);
    Task<int> GetAktivniKlijentiCountAsync();
    Task<int> GetAktivniPrevozniciCountAsync();
}
