using WebApplication1.Utils.DTOs.DashboardDTO;

namespace WebApplication1.Services.DashboardServices;

/// <summary>
/// Service interface for dashboard business logic.
/// Handles data aggregation and transformation.
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Gets complete dashboard overview data.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>Dashboard overview DTO with all metrics.</returns>
    Task<DashboardOverviewDto> GetDashboardOverviewAsync(CancellationToken cancellationToken = default);
}
