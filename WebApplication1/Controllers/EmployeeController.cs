using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.EmployeeServices;
using WebApplication1.Utils.DTOs.UserDTO;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/employees")]
[Authorize]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<EmployeeController> _logger;

    public EmployeeController(IEmployeeService employeeService, ILogger<EmployeeController> logger)
    {
        _employeeService = employeeService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeReadDto>>> GetAll([FromQuery] bool includeInactive = false)
    {
        try
        {
            var employees = await _employeeService.GetAllAsync(includeInactive);
            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri dohvatanju zaposlenih");
            throw;
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeReadDto>> GetById(int id)
    {
        try
        {
            var employee = await _employeeService.GetByIdAsync(id);
            if (employee is null)
                return NotFound(new { message = $"Zaposleni sa ID {id} nije pronađen." });

            return Ok(employee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri dohvatanju zaposlenog ID: {EmployeeId}", id);
            throw;
        }
    }

    [HttpGet("poslovnica/{poslovnicaId:int}")]
    public async Task<ActionResult<IEnumerable<EmployeeReadDto>>> GetByPoslovnica(int poslovnicaId)
    {
        try
        {
            var employees = await _employeeService.GetByPoslovnicaIdAsync(poslovnicaId);
            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri dohvatanju zaposlenih za poslovnicu ID: {PoslovnicaId}", poslovnicaId);
            throw;
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<EmployeeReadDto>> Create([FromBody] EmployeeCreateDto createDto)
    {
        try
        {
            var employee = await _employeeService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = employee.EmployeeId }, employee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri kreiranju zaposlenog za User ID: {UserId}", createDto.UserId);
            throw;
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] EmployeeUpdateDto updateDto)
    {
        try
        {
            updateDto.EmployeeId = id;
            await _employeeService.UpdateAsync(updateDto);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri ažuriranju zaposlenog ID: {EmployeeId}", id);
            throw;
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _employeeService.DeleteAsync(id);
            if (!deleted)
                return BadRequest(new { message = "Zaposleni nije obrisan." });

            return Ok(new { message = "Zaposleni je uspešno obrisan." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri brisanju zaposlenog ID: {EmployeeId}", id);
            throw;
        }
    }
}

