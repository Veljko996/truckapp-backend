using WebApplication1.Services.DashboardServices;
using WebApplication1.Utils.DTOs.DashboardDTO;

namespace WebApplication1.Controllers;

/// Dashboard controller for external dashboard stats.

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(
        IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DashboardStatsDto>> GetExternalDashboardStats(CancellationToken cancellationToken = default)
    {
        var stats = await _dashboardService.GetExternalDashboardStatsAsync(cancellationToken);
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

    [HttpGet("kriticna-vozila")]
    [ProducesResponseType(typeof(List<KriticnoVoziloDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<KriticnoVoziloDto>>> GetKriticnaVozila(
        [FromQuery] int daysThreshold = 7,
        CancellationToken cancellationToken = default)
    {
        if (daysThreshold < 0 || daysThreshold > 60)
            return BadRequest(new { message = "daysThreshold mora biti između 0 i 60." });

        var items = await _dashboardService.GetKriticnaVozilaAsync(daysThreshold, cancellationToken);
        return Ok(items);
    }
}
