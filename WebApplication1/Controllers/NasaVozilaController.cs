using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.NasaVozilaServices;


namespace WebApplication1.Controllers;
[ApiController]
[Authorize]

[Route("api/[controller]")]
public class NasaVozilaController: ControllerBase
{
    private readonly INasaVozilaService _service;
    public NasaVozilaController(INasaVozilaService service)
    {
 
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NasaVozilaReadDto>>> GetAll()
    {
        var vozila = await _service.GetAll();
        return Ok(vozila);
    }

    [HttpGet("{voziloId}")]
    public async Task<ActionResult<NasaVozilaReadDto>> GetById(int voziloId)
    {
        // Service throws NotFoundException if not found - handled by middleware
        var vozilo = await _service.GetById(voziloId);
        return Ok(vozilo);
    }

    [HttpPost]
    public async Task<ActionResult<NasaVozilaReadDto>> Create([FromBody] NasaVozilaCreateDto voziloCreateDto)
    {
        // Service throws ValidationException/ConflictException if invalid - handled by middleware
        var novoVozilo = await _service.Create(voziloCreateDto);
        return CreatedAtAction(nameof(GetById), new { voziloId = novoVozilo.VoziloId }, novoVozilo);
    }

    [HttpPatch("{voziloId}")]
    public async Task<ActionResult<NasaVozilaReadDto>> Update(int voziloId, 
        [FromBody] NasaVozilaUpdateDto updateDto)
    {
        // Service throws NotFoundException if not found - handled by middleware
        var vozilo = await _service.Update(voziloId, updateDto);
        return Ok(vozilo);
    }

    [HttpDelete("{voziloId}")]
    public async Task<IActionResult> Delete(int voziloId)
    {
        // Service throws NotFoundException/ConflictException if invalid - handled by middleware
        await _service.Delete(voziloId);
        return NoContent();
    }

}
