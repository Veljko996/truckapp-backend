using WebApplication1.Services.KlijentServices;
using WebApplication1.Utils.DTOs.KlijentDTO;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class KlijentController : ControllerBase
{
    private readonly IKlijentService _service;

    public KlijentController(IKlijentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<KlijentReadDto>>> GetAll()
    {
        var klijenti = await _service.GetAll();
        return Ok(klijenti);
    }

    [HttpGet("{klijentId}")]
    public async Task<ActionResult<KlijentReadDto>> GetById(int klijentId)
    {
        var klijent = await _service.GetById(klijentId);
        return Ok(klijent);
    }

    [HttpPost]
    public async Task<ActionResult<KlijentReadDto>> Create([FromBody] KlijentCreateDto dto)
    {
        var klijent = await _service.Create(dto);
        return CreatedAtAction(nameof(GetById), new { klijentId = klijent.KlijentId }, klijent);
    }

    [HttpPatch("{klijentId}")]
    public async Task<IActionResult> Update(int klijentId, [FromBody] KlijentUpdateDto dto)
    {
        await _service.Update(klijentId, dto);
        return NoContent();
    }

    [HttpDelete("{klijentId}")]
    public async Task<IActionResult> Delete(int klijentId)
    {
        await _service.Delete(klijentId);
        return NoContent();
    }
}

