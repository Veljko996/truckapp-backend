using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.NalogPrihodiServices;
using WebApplication1.Utils.DTOs.NalogPrihodiDTO;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/nalozi")]
[Authorize(Roles = "Admin,Korisnik")]
public class NalogPrihodiController : ControllerBase
{
    private readonly INalogPrihodiService _service;

    public NalogPrihodiController(INalogPrihodiService service)
    {
        _service = service;
    }

    [HttpGet("{nalogId:int}/prihodi")]
    public async Task<ActionResult<List<NalogPrihodDto>>> GetByNalogId(int nalogId)
    {
        var result = await _service.GetByNalogIdAsync(nalogId);
        return Ok(result);
    }

    [HttpPost("{nalogId:int}/prihodi")]
    public async Task<IActionResult> Create(int nalogId, [FromBody] CreateNalogPrihodDto dto)
    {
        await _service.CreateAsync(nalogId, dto);
        return NoContent();
    }

    [HttpDelete("prihodi/{prihodId:int}")]
    public async Task<IActionResult> Delete(int prihodId)
    {
        await _service.DeleteAsync(prihodId);
        return NoContent();
    }

    [HttpGet("{nalogId:int}/obracun")]
    public async Task<ActionResult<NalogObracunDto>> GetObracun(int nalogId)
    {
        var result = await _service.GetObracunAsync(nalogId);
        return Ok(result);
    }
}
