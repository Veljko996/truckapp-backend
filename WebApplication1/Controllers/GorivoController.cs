using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.GorivoServices;
using WebApplication1.Utils.DTOs.GorivoDTO;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/nasa-vozila")]
[Authorize(Roles = "Admin,Korisnik")]
public class GorivoController : ControllerBase
{
    private readonly IGorivoService _service;

    public GorivoController(IGorivoService service)
    {
        _service = service;
    }

    [HttpGet("{voziloId:int}/gorivo")]
    public async Task<ActionResult<List<GorivoZapisDto>>> GetByVoziloId(int voziloId)
    {
        var result = await _service.GetByVoziloIdAsync(voziloId);
        return Ok(result);
    }

    [HttpPost("{voziloId:int}/gorivo")]
    public async Task<IActionResult> Create(int voziloId, [FromBody] CreateGorivoZapisDto dto)
    {
        await _service.CreateAsync(voziloId, dto);
        return NoContent();
    }

    [HttpDelete("gorivo/{zapisId:int}")]
    public async Task<IActionResult> Delete(int zapisId)
    {
        await _service.DeleteAsync(zapisId);
        return NoContent();
    }

    [HttpGet("gorivo/by-nalog/{nalogId:int}")]
    public async Task<ActionResult<List<GorivoZapisDto>>> GetByNalogId(int nalogId)
    {
        var result = await _service.GetByNalogIdAsync(nalogId);
        return Ok(result);
    }
}
