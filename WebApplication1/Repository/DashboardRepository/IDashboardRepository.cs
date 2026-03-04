using WebApplication1.DataAccess.Models;
using WebApplication1.Utils.DTOs.DashboardDTO;

namespace WebApplication1.Repository.DashboardRepository;

public interface IDashboardRepository
{
    Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
    Task<List<DashboardMonthlyProfitDto>> GetMonthlyProfitAsync(int monthsBack = 12, CancellationToken cancellationToken = default);
    Task<List<NasaVozila>> GetVozilaSaIsticucimDokumentimaAsync(int daysThreshold, CancellationToken cancellationToken = default);
}
