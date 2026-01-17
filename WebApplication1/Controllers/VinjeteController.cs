using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.VinjeteServices;

namespace WebApplication1.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class VinjeteController : ControllerBase
{
    private readonly IVinjeteService _service;

    public VinjeteController(IVinjeteService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VinjetaReadDto>>> GetAll()
    {
        var result = await _service.GetAll();
        
        return Ok(result);
    }

    [HttpGet("{vinjetaId}")]
    public async Task<ActionResult<VinjetaReadDto>> GetById(int vinjetaId)
    {
        var result = await _service.GetById(vinjetaId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Vinjeta>> Create([FromBody] VinjetaCreateDTO dto)
    {
        var vinjeta = await _service.Create(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { vinjetaId = vinjeta.VinjetaId },
            vinjeta
        );
    }

    [HttpPatch("{vinjetaId}")]
    public async Task<IActionResult> Update(int vinjetaId, [FromBody] VinjetaUpdateDto dto)
    {
        await _service.Update(vinjetaId, dto);
        return NoContent();
    }

    [HttpDelete("{vinjetaId}")]
    public async Task<IActionResult> Delete(int vinjetaId)
    {
        // Service throws NotFoundException if not found - handled by middleware
        await _service.Delete(vinjetaId);
        return NoContent();
    }
}
