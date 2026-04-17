using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.KrugServices;
using WebApplication1.Services.TuraServices;
using WebApplication1.Utils.DTOs.KrugDTO;
using WebApplication1.Utils.DTOs.NalogDTO;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/krugovi")]
[Authorize(Roles = "Admin,Korisnik,Vozac")]
public class KrugController : ControllerBase
{
    private readonly IKrugService _service;
    private readonly ITuraService _turaService;

    public KrugController(IKrugService service, ITuraService turaService)
    {
        _service = service;
        _turaService = turaService;
    }

    private bool IsVozac => User.IsInRole("Vozac");
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    [HttpGet]
    public async Task<ActionResult<List<KrugReadDto>>> GetAll()
    {
        int? vozacUserId = IsVozac ? CurrentUserId : null;
        var result = await _service.GetAllAsync(vozacUserId);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<KrugDetailsDto>> GetDetails(int id)
    {
        int? vozacUserId = IsVozac ? CurrentUserId : null;
        var result = await _service.GetDetailsAsync(id, vozacUserId);
        return Ok(result);
    }

    /// <summary>
    /// Server-side agregirani finansijski rezime kruga (Faza 5).
    /// </summary>
    [HttpGet("{id:int}/summary")]
    public async Task<ActionResult<KrugFinancialSummaryDto>> GetSummary(int id)
    {
        int? vozacUserId = IsVozac ? CurrentUserId : null;
        var result = await _service.GetFinancialSummaryAsync(id, vozacUserId);
        return Ok(result);
    }

    /// <summary>
    /// Smart suggest: vrati otvoreni krug za vozilo (ako postoji).
    /// Koristi se u UI-u pre "Kreiraj krug" / "Dodaj u krug" flow-a.
    /// </summary>
    [HttpGet("open-by-vozilo/{voziloId:int}")]
    public async Task<ActionResult<KrugReadDto?>> GetOpenByVozilo(int voziloId)
    {
        int? vozacUserId = IsVozac ? CurrentUserId : null;
        var result = await _service.GetOpenByVoziloAsync(voziloId, vozacUserId);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Korisnik")]
    public async Task<ActionResult<KrugReadDto>> Create([FromBody] CreateKrugDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetDetails), new { id = created.KrugId }, created);
    }

    /// <summary>
    /// Kreira Krug iz postojećeg Naloga (vozilo iz Ture tog Naloga, Tura se odmah povezuje sa Krugom).
    /// </summary>
    [HttpPost("from-nalog/{nalogId:int}")]
    [Authorize(Roles = "Admin,Korisnik")]
    public async Task<ActionResult<KrugReadDto>> CreateFromNalog(int nalogId)
    {
        var created = await _service.CreateFromNalogAsync(nalogId);
        return CreatedAtAction(nameof(GetDetails), new { id = created.KrugId }, created);
    }

    /// <summary>
    /// Unified command: kreira Tura, postavi joj KrugId, ensure-uj interni Nalog.
    /// Frontend dobija jedan Nalog kao odgovor.
    /// </summary>
    [HttpPost("{id:int}/naloge")]
    [Authorize(Roles = "Admin,Korisnik")]
    public async Task<ActionResult<NalogReadDto>> CreateNalogForKrug(int id, [FromBody] CreateNalogForKrugDto dto)
    {
        var created = await _service.CreateNalogForKrugAsync(id, dto);
        return Ok(created);
    }

    [HttpPost("{id:int}/close")]
    [Authorize(Roles = "Admin,Korisnik")]
    public async Task<IActionResult> Close(int id)
    {
        await _service.CloseAsync(id);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Korisnik")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Direktna PATCH ruta za promenu KrugId-a postojeće Ture (dodaj postojeći nalog/turu u krug, ili izbaci).
    /// </summary>
    [HttpPatch("/api/ture/{turaId:int}/krug")]
    [Authorize(Roles = "Admin,Korisnik")]
    public async Task<IActionResult> AssignKrugToTura(int turaId, [FromBody] AssignTuraKrugDto dto)
    {
        await _turaService.AssignKrugAsync(turaId, dto.KrugId);
        return NoContent();
    }
}
