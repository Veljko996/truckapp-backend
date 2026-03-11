using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.NalogTroskoviServices;
using WebApplication1.Utils.DTOs.NalogTroskoviDTO;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/nalozi")]
[Authorize(Roles = "Admin,Korisnik")]
public class NalogTroskoviController : ControllerBase
{
    private readonly INalogTroskoviService _service;

    public NalogTroskoviController(INalogTroskoviService service)
    {
        _service = service;
    }

    [HttpGet("{nalogId:int}/troskovi")]
    public async Task<ActionResult<List<NalogTrosakDto>>> GetByNalogId(int nalogId)
    {
        var result = await _service.GetByNalogIdAsync(nalogId);
        return Ok(result);
    }

    [HttpPost("{nalogId:int}/troskovi")]
    public async Task<IActionResult> Create(int nalogId, [FromBody] CreateNalogTrosakDto dto)
    {
        await _service.CreateAsync(nalogId, dto);
        return NoContent();
    }

    [HttpDelete("troskovi/{trosakId:int}")]
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
