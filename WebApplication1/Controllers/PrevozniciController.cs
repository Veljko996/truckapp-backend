global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.PrevozniciServices;
using WebApplication1.Utils.DTOs.PrevoznikDTO;


namespace WebApplication1.Controllers;
[ApiController]
[Authorize]

[Route("api/[controller]")]
public class PrevozniciController : ControllerBase
{
    private readonly IPrevozniciService _service;
    public PrevozniciController(IPrevozniciService service)
    {

        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PrevoznikDto>>> GetAll()
    {
        var prevoznici = await _service.GetAll();
        return Ok(prevoznici);
    }

    [HttpGet("{prevoznikId}")]
    public async Task<ActionResult<PrevoznikDto>> GetById(int prevoznikId)
    {
        // Service throws NotFoundException if not found - handled by middleware
        var prevoznik = await _service.GetById(prevoznikId);
        return Ok(prevoznik);
    }

    [HttpPost]
    public async Task<ActionResult<PrevoznikDto>> Create([FromBody] CreatePrevoznikDto prevoznikCreateDto)
    {
        // Service throws ValidationException/ConflictException if invalid - handled by middleware
        var noviPrevoznik = await _service.Create(prevoznikCreateDto);
        return CreatedAtAction(nameof(GetById), new { prevoznikId = noviPrevoznik.PrevoznikId }, noviPrevoznik);
    }

    [HttpPatch("{prevoznikId}")]
    public async Task<IActionResult> Update(int prevoznikId,
        [FromBody] UpdatePrevoznikDto updateDto)
    {
        // Service throws NotFoundException if not found - handled by middleware
        await _service.Update(prevoznikId, updateDto);
        return NoContent();
    }

    [HttpDelete("{prevoznikId}")]
    public async Task<IActionResult> Delete(int prevoznikId)
    {
        await _service.Delete(prevoznikId);
        return NoContent();
    }

}
