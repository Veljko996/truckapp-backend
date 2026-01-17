using WebApplication1.Services.NalogServices;
using WebApplication1.Services.QuestPdfServices;
using WebApplication1.Utils.DTOs.NalogDTO;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/nalog")]
[Authorize(Roles = "Admin,Korisnik")]
public class NalogController : ControllerBase
{
    private readonly INalogService _service;
    private readonly IQuestPdfNalogGenerator _questPdfGenerator; // EKSPERIMENTALNO

    public NalogController(INalogService service, IQuestPdfNalogGenerator questPdfGenerator)
    {
        _service = service;
        _questPdfGenerator = questPdfGenerator; // EKSPERIMENTALNO
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
    public async Task<IActionResult> AssignPrevoznik(int id, [FromBody] AssignPrevoznikDto dto)
    {
        await _service.AssignPrevoznik(id, dto);
        return NoContent();
    }

    [HttpPut("{id}/update-business")]
    public async Task<IActionResult> UpdateBusiness(int id, [FromBody] UpdateBusinessFieldsDto dto)
    {
        await _service.UpdateBusiness(id, dto);
        return NoContent();
    }

    [HttpPut("{id}/update-notes")]
    public async Task<IActionResult> UpdateNotes(int id, [FromBody] UpdateNotesDto dto)
    {
        await _service.UpdateNotes(id, dto);
        return NoContent();
    }

    [HttpPut("{id}/update-status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
    {
        await _service.UpdateStatus(id, dto);
        return NoContent();
    }

    [HttpPut("{id}/mark-istovaren")]
    public async Task<IActionResult> MarkIstovaren(int id, [FromBody] MarkIstovarenDto dto)
    {
        await _service.MarkIstovaren(id, dto);
        return NoContent();
    }

    [HttpPut("{id}/storniraj")]
    public async Task<IActionResult> Storniraj(int id)
    {
        await _service.Storniraj(id);
        return NoContent();
    }

    [HttpPut("{id}/ponisti")]
    public async Task<IActionResult> Ponisti(int id)
    {
        await _service.Ponisti(id);
        return NoContent();
    }

    [HttpGet("{id}/document")]
    public async Task<IActionResult> GenerateDocument(
        int id,
        [FromQuery] string template = "mts"
    )
    {
        var pdfBytes = await _questPdfGenerator.GeneratePdfAsync(id, template);

        return File(
            pdfBytes,
            "application/pdf",
            $"Nalog_{id}_{template}.pdf"
        );
    }


}

