using WebApplication1.Services.VrstaNadogradnjeServices;
using WebApplication1.Utils.DTOs.VrstaNadogradnjeDTO;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VrstaNadogradnjeController : ControllerBase
{
    private readonly IVrstaNadogradnjeService _service;

    public VrstaNadogradnjeController(IVrstaNadogradnjeService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VrstaNadogradnjeReadDto>>> GetAll()
    {
        var vrsteNadogradnje = await _service.GetAll();
        return Ok(vrsteNadogradnje);
    }

    [HttpGet("{vrstaNadogradnjeId}")]
    public async Task<ActionResult<VrstaNadogradnjeReadDto>> GetById(int vrstaNadogradnjeId)
    {
        // Service throws NotFoundException if not found - handled by middleware
        var vrstaNadogradnje = await _service.GetById(vrstaNadogradnjeId);
        return Ok(vrstaNadogradnje);
    }
}

