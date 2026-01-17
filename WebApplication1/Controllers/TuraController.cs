using WebApplication1.Services.TuraServices;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Korisnik")]
public class TuraController : ControllerBase
{
    private readonly ITuraService _service;

    public TuraController(ITuraService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TuraReadDto>>> GetAll()
    {
        var result = await _service.GetAll();
        return Ok(result);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<TuraReadDto>> GetById(int id)
    {
        var result = await _service.GetById(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<TuraReadDto>> Create([FromBody] CreateTuraDto dto)
    {
        var created = await _service.Create(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.TuraId }, created);
    }

	[HttpPatch("{id}/basic")]
	public async Task<IActionResult> UpdateBasic(int id, UpdateTuraDto dto)
	{
		await _service.UpdateBasic(id, dto);
		return NoContent(); // 204
	}

	// PATCH 2 – BUSINESS (komercijalni)
	[HttpPatch("{id}/business")]
    public async Task<ActionResult<TuraReadDto>> UpdateBusiness(int id, [FromBody] UpdateTureBusinessDto dto)
    {
		await _service.UpdateBusiness(id, dto);
		return NoContent(); // 204
	}

    // PATCH 3 – NOTES (napomene/carinjenje)
    [HttpPatch("{id}/notes")]
    public async Task<ActionResult<TuraReadDto>> UpdateNotes(int id, [FromBody] UpdateTuraNotesDto dto)
    {
		await _service.UpdateNotes(id, dto);
		return NoContent(); // 204
	}

    
    [HttpPatch("{id}/status")]
    public async Task<ActionResult<TuraReadDto>> UpdateStatus(int id, [FromBody] UpdateTuraStatusDto dto)
    {
		await _service.UpdateStatus(id, dto);
		return NoContent(); // 204
	}

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.Delete(id);
        return NoContent();
    }
}
