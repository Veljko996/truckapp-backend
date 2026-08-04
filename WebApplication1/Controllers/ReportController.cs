using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.ReportServices;
using WebApplication1.Utils.DTOs.ReportDTO;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = "Admin,Korisnik")]
public class ReportController : ControllerBase
{
    private readonly IReportService _service;

    public ReportController(IReportService service)
    {
        _service = service;
    }

    /// <summary>
    /// Izveštaj o zatvorenim krugovima u datumskom opsegu (po datumu zatvaranja kruga):
    /// prikaz po krugu + zbirno po vozilu + ukupno, sve po valuti.
    /// </summary>
    [HttpGet("krugovi")]
    public async Task<ActionResult<KrugoviReportDto>> GetKrugoviReport(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] int? voziloId = null)
    {
        if (from == default || to == default)
            return BadRequest(new { message = "Datum od i datum do su obavezni." });

        // Uključi ceo krajnji dan (do 23:59:59.999) ako je poslato samo datum bez vremena.
        if (to.TimeOfDay == TimeSpan.Zero)
            to = to.Date.AddDays(1).AddTicks(-1);

        if (from > to)
            return BadRequest(new { message = "Datum od ne može biti posle datuma do." });

        if ((to - from).TotalDays > 366)
            return BadRequest(new { message = "Opseg izveštaja ne može biti duži od godinu dana." });

        var result = await _service.GetKrugoviReportAsync(from, to, voziloId);
        return Ok(result);
    }
}
