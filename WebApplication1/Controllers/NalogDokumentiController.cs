using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.NalogDokumentiServices;
using WebApplication1.Utils.DTOs.NalogDokumentiDTO;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/nalozi")]
[Authorize(Roles = "Admin,Korisnik")]
public class NalogDokumentiController : ControllerBase
{
    private readonly INalogDokumentiService _service;

    public NalogDokumentiController(INalogDokumentiService service)
    {
        _service = service;
    }

    [HttpGet("{nalogId:int}/dokumenti")]
    public async Task<ActionResult<List<NalogDokumentDto>>> GetByNalogId(int nalogId)
    {
        var result = await _service.GetByNalogIdAsync(nalogId);
        return Ok(result);
    }

    [HttpPost("{nalogId:int}/dokumenti")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(26_214_400)] // 25 MB
    public async Task<ActionResult<NalogDokumentDto>> Upload(int nalogId, [FromForm] UploadNalogDokumentDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.UploadAsync(nalogId, dto, cancellationToken);
        return Ok(result);
    }

    [HttpGet("dokumenti/{dokumentId:int}/download")]
    public async Task<IActionResult> Download(int dokumentId)
    {
        var (stream, contentType, fileName) = await _service.DownloadAsync(dokumentId);
        return File(stream, contentType, fileName);
    }

    [HttpDelete("dokumenti/{dokumentId:int}")]
    public async Task<IActionResult> Delete(int dokumentId)
    {
        await _service.DeleteAsync(dokumentId);
        return NoContent();
    }

    [HttpGet("tipovi-dokumenata")]
    public async Task<ActionResult<List<TipDokumentaDto>>> GetTipoviDokumenata()
    {
        var result = await _service.GetAllTipoviAsync();
        return Ok(result);
    }
}
