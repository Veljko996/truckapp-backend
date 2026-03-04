using WebApplication1.Services.DashboardServices;
using WebApplication1.Utils.DTOs.DashboardDTO;

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

    [HttpGet]
    [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats(CancellationToken cancellationToken = default)
    {
        var stats = await _dashboardService.GetDashboardStatsAsync(cancellationToken);
        return Ok(stats);
    }

    [HttpGet("profit-by-month")]
    [ProducesResponseType(typeof(List<DashboardMonthlyProfitDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<DashboardMonthlyProfitDto>>> GetProfitByMonth(
        [FromQuery] int monthsBack = 12,
        CancellationToken cancellationToken = default)
    {
        if (monthsBack <= 0 || monthsBack > 60)
            return BadRequest(new { message = "monthsBack mora biti između 1 i 60." });

        var items = await _dashboardService.GetMonthlyProfitAsync(monthsBack, cancellationToken);
        return Ok(items);
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
