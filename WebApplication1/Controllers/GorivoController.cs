using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.GorivoServices;
using WebApplication1.Services.NalogVozacAccessServices;
using WebApplication1.Utils.DTOs.GorivoDTO;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/nasa-vozila")]
[Authorize(Roles = "Admin,Korisnik,Vozac")]
public class GorivoController : ControllerBase
{
    private readonly IGorivoService _service;
    private readonly INalogVozacAccessService _vozacAccess;

    public GorivoController(IGorivoService service, INalogVozacAccessService vozacAccess)
    {
        _service = service;
        _vozacAccess = vozacAccess;
    }

    private bool IsVozac => User.IsInRole("Vozac");
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    [HttpGet("{voziloId:int}/gorivo")]
    public async Task<ActionResult<List<GorivoZapisDto>>> GetByVoziloId(int voziloId)
    {
        if (IsVozac && !await _vozacAccess.CanAccessVoziloAsync(CurrentUserId, voziloId))
            return Forbid();

        var result = await _service.GetByVoziloIdAsync(voziloId);
        return Ok(result);
    }

    [HttpPost("{voziloId:int}/gorivo")]
    public async Task<IActionResult> Create(int voziloId, [FromBody] CreateGorivoZapisDto dto)
    {
        if (IsVozac && !await _vozacAccess.CanAccessVoziloAsync(CurrentUserId, voziloId))
            return Forbid();

        await _service.CreateAsync(voziloId, dto);
        return NoContent();
    }

    [HttpDelete("gorivo/{zapisId:int}")]
    [Authorize(Roles = "Admin,Korisnik")]
    public async Task<IActionResult> Delete(int zapisId)
    {
        await _service.DeleteAsync(zapisId);
        return NoContent();
    }

    [HttpGet("gorivo/by-nalog/{nalogId:int}")]
    public async Task<ActionResult<List<GorivoZapisDto>>> GetByNalogId(int nalogId)
    {
        if (IsVozac && !await _vozacAccess.CanAccessNalogAsync(CurrentUserId, nalogId))
            return Forbid();

        var result = await _service.GetByNalogIdAsync(nalogId);
        return Ok(result);
    }
}
