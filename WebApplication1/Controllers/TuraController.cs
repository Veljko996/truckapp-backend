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

    [HttpPatch("{id}")]
    public async Task<ActionResult<TuraReadDto>> Update(int id, [FromBody] UpdateTuraDto dto)
    {
        var result = await _service.Update(id, dto);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.Delete(id);
        return NoContent();
    }
}
