using WebApplication1.Services.NalogServices;
using WebApplication1.Utils.DTOs.NalogDTO;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/nalog")]
[Authorize(Roles = "Admin,Korisnik")]
public class NalogController : ControllerBase
{
    private readonly INalogService _service;

    public NalogController(INalogService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NalogReadDto>>> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<NalogReadDto>> GetById(int id)
    {
        var result = await _service.GetById(id);
        return Ok(result);
    }

    [HttpPost("create/{turaId}")]
    public async Task<ActionResult<NalogReadDto>> Create(int turaId, [FromBody] CreateNalogDto dto)
    {
        var created = await _service.Create(turaId, dto);
        return CreatedAtAction(nameof(GetById), new { id = created.NalogId }, created);
    }

    [HttpPut("{id}/assign-prevoznik")]
    public async Task<ActionResult<NalogReadDto>> AssignPrevoznik(int id, [FromBody] AssignPrevoznikDto dto)
    {
        var result = await _service.AssignPrevoznik(id, dto);
        return Ok(result);
    }

    [HttpPut("{id}/update-business")]
    public async Task<ActionResult<NalogReadDto>> UpdateBusiness(int id, [FromBody] UpdateBusinessFieldsDto dto)
    {
        var result = await _service.UpdateBusiness(id, dto);
        return Ok(result);
    }

    [HttpPut("{id}/update-notes")]
    public async Task<ActionResult<NalogReadDto>> UpdateNotes(int id, [FromBody] UpdateNotesDto dto)
    {
        var result = await _service.UpdateNotes(id, dto);
        return Ok(result);
    }

    [HttpPut("{id}/update-status")]
    public async Task<ActionResult<NalogReadDto>> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
    {
        var result = await _service.UpdateStatus(id, dto);
        return Ok(result);
    }

    [HttpPut("{id}/mark-istovaren")]
    public async Task<ActionResult<NalogReadDto>> MarkIstovaren(int id, [FromBody] MarkIstovarenDto dto)
    {
        var result = await _service.MarkIstovaren(id, dto);
        return Ok(result);
    }

    [HttpPut("{id}/storniraj")]
    public async Task<ActionResult<NalogReadDto>> Storniraj(int id)
    {
        var result = await _service.Storniraj(id);
        return Ok(result);
    }

    [HttpPut("{id}/ponisti")]
    public async Task<ActionResult<NalogReadDto>> Ponisti(int id)
    {
        var result = await _service.Ponisti(id);
        return Ok(result);
    }

    [HttpGet("{id}/document")]
    public async Task<IActionResult> GenerateDocument(
    int id,
    [FromQuery] string template = "mts",
    [FromQuery] string format = "html"
)
    {
        var bytes = await _service.GenerateHtmlAsync(id, template);

        return format.ToLowerInvariant() switch
        {
            "doc" => File(
                bytes,
                "application/msword",
                $"Nalog_{id}.doc"
            ),

            "html" => File(
                bytes,
                "text/html; charset=utf-8",
                $"Nalog_{id}.html"
            ),

            _ => BadRequest(
                "Nepoznat format. Dozvoljeno: html, doc."
            )
        };
    }


}

