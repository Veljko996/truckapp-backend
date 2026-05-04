using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.DrzavaServices;
using WebApplication1.Utils.DTOs.DrzavaDTO;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DrzaveController : ControllerBase
{
    private readonly IDrzavaService _service;

    public DrzaveController(IDrzavaService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DrzavaDto>>> GetAll()
    {
        var drzave = await _service.GetAllAsync();
        return Ok(drzave);
    }
}
