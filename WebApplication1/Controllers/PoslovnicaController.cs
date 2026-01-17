using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.PoslovnicaServices;
using WebApplication1.Utils.DTOs.PoslovnicaDTO;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PoslovnicaController : ControllerBase
{
    private readonly IPoslovnicaService _poslovnicaService;
    private readonly ILogger<PoslovnicaController> _logger;

    public PoslovnicaController(IPoslovnicaService poslovnicaService, ILogger<PoslovnicaController> logger)
    {
        _poslovnicaService = poslovnicaService;
        _logger = logger;
    }

    /// <summary>
    /// Vraća sve poslovnice
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PoslovnicaReadDto>>> GetAllPoslovnice()
    {
        try
        {
            var poslovnice = await _poslovnicaService.GetAllPoslovniceAsync();
            return Ok(poslovnice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri dohvatanju poslovnica");
            throw;
        }
    }

    /// <summary>
    /// Vraća poslovnicu po ID-u
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PoslovnicaReadDto>> GetPoslovnicaById(int id)
    {
        try
        {
            var poslovnica = await _poslovnicaService.GetPoslovnicaByIdAsync(id);
            if (poslovnica is null)
                return NotFound(new { message = $"Poslovnica sa ID {id} nije pronađena." });

            return Ok(poslovnica);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri dohvatanju poslovnice ID: {PoslovnicaId}", id);
            throw;
        }
    }

    /// <summary>
    /// Kreira novu poslovnicu
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PoslovnicaReadDto>> CreatePoslovnica([FromBody] PoslovnicaCreateDto createDto)
    {
        try
        {
            var poslovnica = await _poslovnicaService.CreatePoslovnicaAsync(createDto);
            return CreatedAtAction(nameof(GetPoslovnicaById), new { id = poslovnica.PoslovnicaId }, poslovnica);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri kreiranju poslovnice: {PJ}", createDto.PJ);
            throw;
        }
    }

    /// <summary>
    /// Ažurira poslovnicu
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdatePoslovnica(int id, [FromBody] PoslovnicaUpdateDto updateDto)
    {
        try
        {
            updateDto.PoslovnicaId = id;
            await _poslovnicaService.UpdatePoslovnicaAsync(updateDto);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri ažuriranju poslovnice ID: {PoslovnicaId}", id);
            throw;
        }
    }

    /// <summary>
    /// Briše poslovnicu
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeletePoslovnica(int id)
    {
        try
        {
            var deleted = await _poslovnicaService.DeletePoslovnicaAsync(id);
            if (!deleted)
                return BadRequest(new { message = "Poslovnica nije obrisana." });

            return Ok(new { message = "Poslovnica je uspešno obrisana." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri brisanju poslovnice ID: {PoslovnicaId}", id);
            throw;
        }
    }
}

