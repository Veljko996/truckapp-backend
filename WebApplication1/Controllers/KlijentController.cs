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
        // Service throws NotFoundException if not found - handled by middleware
        var klijent = await _service.GetById(klijentId);
        return Ok(klijent);
    }
}

