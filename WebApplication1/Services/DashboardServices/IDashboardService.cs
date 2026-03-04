using WebApplication1.Utils.DTOs.DashboardDTO;

namespace WebApplication1.Services.DashboardServices;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
    Task<List<DashboardMonthlyProfitDto>> GetMonthlyProfitAsync(int monthsBack = 12, CancellationToken cancellationToken = default);
    Task<DashboardOverviewDto> GetDashboardOverviewAsync(CancellationToken cancellationToken = default);
}
