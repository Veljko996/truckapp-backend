using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.DashboardServices;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    /// <summary>
    /// Vraća sve podatke potrebne za dashboard prikaz
    /// </summary>
    [HttpGet("overview")]
    public async Task<ActionResult> GetDashboardOverview()
    {
        try
        {
            var dashboard = await _dashboardService.GetDashboardOverviewAsync();
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri dohvatanju dashboard podataka");
            throw;
        }
    }
}

