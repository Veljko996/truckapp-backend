using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.GorivoServices;
using WebApplication1.Services.NalogVozacAccessServices;
using WebApplication1.Utils.DTOs.GorivoDTO;

namespace WebApplication1.Controllers;

[ApiController]
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

    // ── Po vozilu ────────────────────────────────────────────────────────────

    [HttpGet("api/nasa-vozila/{voziloId:int}/gorivo")]
    public async Task<ActionResult<List<GorivoZapisDto>>> GetByVoziloId(int voziloId)
    {
        if (IsVozac && !await _vozacAccess.CanAccessVoziloAsync(CurrentUserId, voziloId))
            return Forbid();

        var result = await _service.GetByVoziloIdAsync(voziloId);
        return Ok(result);
    }

    [HttpPost("api/nasa-vozila/{voziloId:int}/gorivo")]
    public async Task<IActionResult> CreateForVozilo(int voziloId, [FromBody] CreateGorivoZapisDto dto)
    {
        if (IsVozac && !await _vozacAccess.CanAccessVoziloAsync(CurrentUserId, voziloId))
            return Forbid();

        await _service.CreateAsync(voziloId, dto);
        return NoContent();
    }

    // ── Po nalogu ────────────────────────────────────────────────────────────

    [HttpGet("api/nasa-vozila/gorivo/by-nalog/{nalogId:int}")]
    public async Task<ActionResult<List<GorivoZapisDto>>> GetByNalogId(int nalogId)
    {
        if (IsVozac && !await _vozacAccess.CanAccessNalogAsync(CurrentUserId, nalogId))
            return Forbid();

        var result = await _service.GetByNalogIdAsync(nalogId);
        return Ok(result);
    }

    // ── Po krugu ─────────────────────────────────────────────────────────────

    [HttpGet("api/krugovi/{krugId:int}/gorivo")]
    public async Task<ActionResult<List<GorivoZapisDto>>> GetByKrugId(int krugId)
    {
        var result = await _service.GetByKrugIdAsync(krugId);
        return Ok(result);
    }

    // ── Brisanje ─────────────────────────────────────────────────────────────

    [HttpDelete("api/nasa-vozila/gorivo/{zapisId:int}")]
    [Authorize(Roles = "Admin,Korisnik")]
    public async Task<IActionResult> Delete(int zapisId)
    {
        await _service.DeleteAsync(zapisId);
        return NoContent();
    }
}
