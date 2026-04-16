using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using WebApplication1.Services.NalogServices;
using WebApplication1.Services.NalogVozacAccessServices;
using WebApplication1.Services.QuestPdfServices;
using WebApplication1.Utils.DTOs.NalogDTO;
using WebApplication1.Utils.Tenant;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/nalog")]
[Authorize(Roles = "Admin,Korisnik,Vozac")]
public class NalogController : ControllerBase
{
    private readonly INalogService _service;
    private readonly IQuestPdfNalogGenerator _questPdfGenerator;
    private readonly IPdfTemplatePolicy _pdfTemplatePolicy;
    private readonly ITenantProvider _tenantProvider;
    private readonly INalogVozacAccessService _vozacAccess;

    public NalogController(
        INalogService service,
        IQuestPdfNalogGenerator questPdfGenerator,
        IPdfTemplatePolicy pdfTemplatePolicy,
        ITenantProvider tenantProvider,
        INalogVozacAccessService vozacAccess)
    {
        _service = service;
        _questPdfGenerator = questPdfGenerator;
        _pdfTemplatePolicy = pdfTemplatePolicy;
        _tenantProvider = tenantProvider;
        _vozacAccess = vozacAccess;
    }

    private bool IsVozac => User.IsInRole("Vozac");
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NalogReadDto>>> GetAll()
    {
        int? vozacUserId = IsVozac ? CurrentUserId : null;
        var result = await _service.GetAllAsync(vozacUserId);
        return Ok(result);
    }

    [HttpGet("interni")]
    public async Task<ActionResult<IEnumerable<NalogReadDto>>> GetInterni()
    {
        int? vozacUserId = IsVozac ? CurrentUserId : null;
        var result = await _service.GetInterniAsync(vozacUserId);
        return Ok(result);
    }

    [HttpGet("kasnjenja-istovara")]
    [Authorize(Roles = "Admin,Korisnik")]
    public async Task<ActionResult<IEnumerable<NalogReadDto>>> GetKasnjenjaIstovara()
    {
        var result = await _service.GetNaloziSaIstovaromUKasnjenjuAsync();
        return Ok(result ?? Array.Empty<NalogReadDto>());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<NalogReadDto>> GetById(int id)
    {
        if (IsVozac && !await _vozacAccess.CanAccessNalogAsync(CurrentUserId, id))
            return Forbid();

        var result = await _service.GetById(id);
        return Ok(result);
    }

    [HttpPost("create/{turaId}")]
    [Authorize(Roles = "Admin,Korisnik")]
    public async Task<ActionResult<NalogReadDto>> Create(int turaId, [FromBody] CreateNalogDto dto)
    {
        var created = await _service.Create(turaId, dto);
        return CreatedAtAction(nameof(GetById), new { id = created.NalogId }, created);
    }

    [HttpPut("{id}/assign-prevoznik")]
    [Authorize(Roles = "Admin,Korisnik")]
    public async Task<IActionResult> AssignPrevoznik(int id, [FromBody] AssignPrevoznikDto dto)
    {
        await _service.AssignPrevoznik(id, dto);
        return NoContent();
    }

    [HttpPut("{id}/update-business")]
    [Authorize(Roles = "Admin,Korisnik")]
    public async Task<IActionResult> UpdateBusiness(int id, [FromBody] UpdateBusinessFieldsDto dto)
    {
        await _service.UpdateBusiness(id, dto);
        return NoContent();
    }

    [HttpPut("{id}/update-notes")]
    [Authorize(Roles = "Admin,Korisnik")]
    public async Task<IActionResult> UpdateNotes(int id, [FromBody] UpdateNotesDto dto)
    {
        await _service.UpdateNotes(id, dto);
        return NoContent();
    }

    [HttpPut("{id}/update-status")]
    [Authorize(Roles = "Admin,Korisnik")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
    {
        await _service.UpdateStatus(id, dto);
        return NoContent();
    }

    [HttpPut("{id}/mark-istovaren")]
    [Authorize(Roles = "Admin,Korisnik")]
    public async Task<IActionResult> MarkIstovaren(int id, [FromBody] MarkIstovarenDto dto)
    {
        await _service.MarkIstovaren(id, dto);
        return NoContent();
    }

    [HttpPut("{id}/storniraj")]
    [Authorize(Roles = "Admin,Korisnik")]
    public async Task<IActionResult> Storniraj(int id)
    {
        await _service.Storniraj(id);
        return NoContent();
    }

    [HttpPut("{id}/ponisti")]
    [Authorize(Roles = "Admin,Korisnik")]
    public async Task<IActionResult> Ponisti(int id)
    {
        await _service.Ponisti(id);
        return NoContent();
    }

    /// <summary>Šabloni PDF naloga dozvoljeni za trenutni tenant (isti izvor kao validacija pri preuzimanju).</summary>
    [HttpGet("document-templates")]
    [Authorize(Roles = "Admin,Korisnik")]
    public ActionResult<IReadOnlyList<string>> GetDocumentTemplates()
    {
        var list = _pdfTemplatePolicy.GetAllowedTemplates(_tenantProvider.CurrentTenantId);
        return Ok(list);
    }

    [HttpGet("{id:int}/document")]
    [Authorize(Roles = "Admin,Korisnik")]
    public async Task<IActionResult> GenerateDocument(
        int id,
        [FromQuery] string template = "mts"
    )
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!_pdfTemplatePolicy.IsAllowed(tenantId, template))
        {
            return Problem(
                detail: "Izabrani šablon nije dozvoljen za vašu firmu.",
                statusCode: StatusCodes.Status403Forbidden);
        }

        var pdfBytes = await _questPdfGenerator.GeneratePdfAsync(id, template);

        return File(
            pdfBytes,
            "application/pdf",
            $"Nalog_{id}_{template}.pdf"
        );
    }
}

