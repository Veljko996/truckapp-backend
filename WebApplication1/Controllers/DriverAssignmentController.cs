using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.DriverAssignmentServices;
using WebApplication1.Utils.DTOs.DriverAssignmentDTO;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/nasa-vozila")]
[Authorize(Roles = "Admin,Korisnik")]
public class DriverAssignmentController : ControllerBase
{
    private readonly IDriverAssignmentService _service;

    public DriverAssignmentController(IDriverAssignmentService service)
    {
        _service = service;
    }

    [HttpGet("{voziloId:int}/vozaci")]
    public async Task<ActionResult<List<DriverAssignmentReadDto>>> GetActiveByVoziloId(int voziloId)
    {
        var result = await _service.GetActiveByVoziloIdAsync(voziloId);
        return Ok(result);
    }

    // Batch endpoint for fleet cards
    // Example: /api/nasa-vozila/vozaci/active?voziloIds=1&voziloIds=2
    [HttpGet("vozaci/active")]
    public async Task<ActionResult<List<DriverAssignmentReadDto>>> GetActiveForVozila(
        [FromQuery] int[] voziloIds)
    {
        var result = await _service.GetActiveByVoziloIdsAsync(voziloIds);
        return Ok(result);
    }

    [HttpGet("{voziloId:int}/vozaci/history")]
    public async Task<ActionResult<List<DriverAssignmentReadDto>>> GetHistoryByVoziloId(int voziloId)
    {
        var result = await _service.GetHistoryByVoziloIdAsync(voziloId);
        return Ok(result);
    }

    [HttpGet("vozaci/available")]
    public async Task<ActionResult<List<AvailableDriverDto>>> GetAvailableDrivers([FromQuery] int? voziloId = null)
    {
        var result = await _service.GetAvailableDriversAsync(voziloId);
        return Ok(result);
    }

    [HttpPost("{voziloId:int}/vozaci/assign")]
    public async Task<IActionResult> Assign(int voziloId, [FromBody] AssignDriverDto dto)
    {
        await _service.AssignAsync(voziloId, dto);
        return NoContent();
    }

    [HttpPost("{voziloId:int}/vozaci/unassign")]
    public async Task<IActionResult> Unassign(int voziloId, [FromBody] UnassignDriverDto dto)
    {
        await _service.UnassignAsync(voziloId, dto);
        return NoContent();
    }
}

