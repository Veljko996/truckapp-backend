using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.KrugTroskoviServices;
using WebApplication1.Utils.DTOs.KrugTroskoviDTO;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/krugovi")]
[Authorize(Roles = "Admin,Korisnik")]
public class KrugTroskoviController : ControllerBase
{
    private readonly IKrugTroskoviService _service;

    public KrugTroskoviController(IKrugTroskoviService service)
    {
        _service = service;
    }

    [HttpGet("{krugId:int}/troskovi")]
    public async Task<ActionResult<List<KrugTrosakDto>>> GetByKrugId(int krugId)
    {
        var result = await _service.GetByKrugIdAsync(krugId);
        return Ok(result);
    }

    [HttpPost("{krugId:int}/troskovi")]
    public async Task<IActionResult> Create(int krugId, [FromBody] CreateKrugTrosakDto dto)
    {
        await _service.CreateAsync(krugId, dto);
        return NoContent();
    }

    [HttpDelete("troskovi/{krugTrosakId:int}")]
    public async Task<IActionResult> Delete(int krugTrosakId)
    {
        await _service.DeleteAsync(krugTrosakId);
        return NoContent();
    }
}
