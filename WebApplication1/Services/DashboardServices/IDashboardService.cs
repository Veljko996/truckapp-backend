using WebApplication1.Utils.DTOs.DashboardDTO;

namespace WebApplication1.Services.DashboardServices;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetExternalDashboardStatsAsync(CancellationToken cancellationToken = default);
    Task<List<DashboardMonthlyProfitDto>> GetMonthlyProfitAsync(int monthsBack = 12, CancellationToken cancellationToken = default);
    Task<List<KriticnoVoziloDto>> GetKriticnaVozilaAsync(int daysThreshold = 7, CancellationToken cancellationToken = default);
}
