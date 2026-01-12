using WebApplication1.Services.DashboardServices;

namespace WebApplication1.Controllers;

/// Dashboard controller providing overview metrics and statistics.

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IDashboardService dashboardService,
        ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    [HttpGet("overview")]
    [ProducesResponseType(typeof(Utils.DTOs.DashboardDTO.DashboardOverviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Utils.DTOs.DashboardDTO.DashboardOverviewDto>> GetDashboardOverview(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching dashboard overview data");

            var dashboard = await _dashboardService.GetDashboardOverviewAsync(cancellationToken);

            _logger.LogInformation("Dashboard overview data fetched successfully");

            return Ok(dashboard);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Dashboard overview request was cancelled");
            return StatusCode(StatusCodes.Status408RequestTimeout, new { message = "Request timeout" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dashboard overview");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error fetching dashboard data. Please try again later." });
        }
    }
}
