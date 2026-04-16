using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.NalogTroskoviServices;
using WebApplication1.Services.NalogVozacAccessServices;
using WebApplication1.Utils.DTOs.NalogTroskoviDTO;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/nalozi")]
[Authorize(Roles = "Admin,Korisnik,Vozac")]
public class NalogTroskoviController : ControllerBase
{
    private readonly INalogTroskoviService _service;
    private readonly INalogVozacAccessService _vozacAccess;

    public NalogTroskoviController(INalogTroskoviService service, INalogVozacAccessService vozacAccess)
    {
        _service = service;
        _vozacAccess = vozacAccess;
    }

    private bool IsVozac => User.IsInRole("Vozac");
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    [HttpGet("{nalogId:int}/troskovi")]
    public async Task<ActionResult<List<NalogTrosakDto>>> GetByNalogId(int nalogId)
    {
        if (IsVozac && !await _vozacAccess.CanAccessNalogAsync(CurrentUserId, nalogId))
            return Forbid();

        var result = await _service.GetByNalogIdAsync(nalogId);
        return Ok(result);
    }

    [HttpPost("{nalogId:int}/troskovi")]
    public async Task<IActionResult> Create(int nalogId, [FromBody] CreateNalogTrosakDto dto)
    {
        if (IsVozac && !await _vozacAccess.CanAccessNalogAsync(CurrentUserId, nalogId))
            return Forbid();

        await _service.CreateAsync(nalogId, dto);
        return NoContent();
    }

    [HttpDelete("troskovi/{trosakId:int}")]
    [Authorize(Roles = "Admin,Korisnik")]
    public async Task<IActionResult> Delete(int trosakId)
    {
        await _service.DeleteAsync(trosakId);
        return NoContent();
    }

    [HttpGet("tipovi-troskova")]
    public async Task<ActionResult<List<TipTroskaDto>>> GetTipoviTroskova()
    {
        var result = await _service.GetAllTipoviAsync();
        return Ok(result);
    }
}
