using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.TuraServices;
using WebApplication1.Utils.Exceptions;

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
        // Service throws NotFoundException if not found - handled by middleware
        var result = await _service.GetById(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<TuraReadDto>> Create([FromBody] CreateTuraDto dto)
    {
        // Model validation is handled by [ApiController] attribute
        // Service exceptions are handled by ErrorHandlerMiddleware
        // Exceptions are re-thrown here so middleware can handle them properly
        var created = await _service.Create(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.TuraId }, created);
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<TuraReadDto>> Update(int id, [FromBody] UpdateTuraDto dto)
    {
        // Service throws NotFoundException/ValidationException if invalid - handled by middleware
        var result = await _service.Update(id, dto);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        // Service throws NotFoundException/ConflictException if invalid - handled by middleware
        await _service.Delete(id);
        return NoContent();
    }
}
