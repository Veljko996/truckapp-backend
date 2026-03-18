using WebApplication1.DataAccess.Models;
using WebApplication1.Repository.EmployeeRepository;
using WebApplication1.Repository.UserRepository;
using WebApplication1.Utils.DTOs.UserDTO;

namespace WebApplication1.Services.EmployeeServices;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(
        IEmployeeRepository employeeRepository,
        IUserRepository userRepository,
        ILogger<EmployeeService> logger)
    {
        _employeeRepository = employeeRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<EmployeeReadDto?> GetByIdAsync(int employeeId)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        return employee is null ? null : MapToEmployeeReadDto(employee);
    }

    public async Task<EmployeeReadDto?> GetByUserIdAsync(int userId)
    {
        var employee = await _employeeRepository.GetByUserIdAsync(userId);
        return employee is null ? null : MapToEmployeeReadDto(employee);
    }

    public async Task<IEnumerable<EmployeeReadDto>> GetAllAsync(bool includeInactive = false)
    {
        IEnumerable<Employee> employees = includeInactive
            ? await _employeeRepository.GetAllAsync()
            : await _employeeRepository.GetActiveAsync();

        return employees.Select(MapToEmployeeReadDto);
    }

    public async Task<IEnumerable<EmployeeReadDto>> GetByPoslovnicaIdAsync(int poslovnicaId)
    {
        var employees = await _employeeRepository.GetByPoslovnicaIdAsync(poslovnicaId);
        return employees.Select(MapToEmployeeReadDto);
    }

    public async Task<EmployeeReadDto> CreateAsync(EmployeeCreateDto createDto)
    {
        var user = await _userRepository.GetByIdAsync(createDto.UserId);
        if (user is null)
            throw new NotFoundException("UserNotFound", $"Korisnik sa ID {createDto.UserId} nije pronađen.");

        var existing = await _employeeRepository.GetByUserIdAsync(createDto.UserId);
        if (existing is not null)
            throw new ConflictException("EmployeeExists", $"Korisnik sa ID {createDto.UserId} već ima zapis zaposlenog.");

        if (!string.IsNullOrWhiteSpace(createDto.EmployeeNumber)
            && await _employeeRepository.EmployeeNumberExistsAsync(createDto.EmployeeNumber))
            throw new ConflictException("EmployeeNumberExists", $"Broj zaposlenog '{createDto.EmployeeNumber}' već postoji.");

        var employee = new Employee
        {
            UserId = createDto.UserId,
            EmployeeNumber = createDto.EmployeeNumber,
            Position = createDto.Position,
            Department = createDto.Department,
            PoslovnicaId = createDto.PoslovnicaId,
            HireDate = createDto.HireDate ?? DateTime.UtcNow,
            LicenseNumber = createDto.LicenseNumber,
            LicenseExpiryDate = createDto.LicenseExpiryDate,
            LicenseCategory = createDto.LicenseCategory,
            Salary = createDto.Salary,
            Notes = createDto.Notes,
            IsActive = createDto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _employeeRepository.AddAsync(employee);
        var saved = await _employeeRepository.SaveChangesAsync();
        if (!saved)
            throw new ConflictException("SaveFailed", "Došlo je do greške prilikom kreiranja zaposlenog.");

        _logger.LogInformation("Zaposleni kreiran: User ID {UserId}, Employee ID {EmployeeId}", employee.UserId, employee.EmployeeId);

        var createdEmployee = await _employeeRepository.GetByIdAsync(employee.EmployeeId);
        return MapToEmployeeReadDto(createdEmployee!);
    }

    public async Task UpdateAsync(EmployeeUpdateDto updateDto)
    {
        var employee = await _employeeRepository.GetByIdAsync(updateDto.EmployeeId);
        if (employee is null)
            throw new NotFoundException("EmployeeNotFound", $"Zaposleni sa ID {updateDto.EmployeeId} nije pronađen.");

        if (!string.IsNullOrWhiteSpace(updateDto.EmployeeNumber)
            && await _employeeRepository.EmployeeNumberExistsForOtherEmployeeAsync(updateDto.EmployeeNumber, updateDto.EmployeeId))
            throw new ConflictException("EmployeeNumberExists", $"Broj zaposlenog '{updateDto.EmployeeNumber}' već postoji.");

        if (updateDto.EmployeeNumber is not null)
            employee.EmployeeNumber = updateDto.EmployeeNumber;
        if (updateDto.Position is not null)
            employee.Position = updateDto.Position;
        if (updateDto.Department is not null)
            employee.Department = updateDto.Department;
        if (updateDto.PoslovnicaId.HasValue)
            employee.PoslovnicaId = updateDto.PoslovnicaId;
        if (updateDto.HireDate.HasValue)
            employee.HireDate = updateDto.HireDate;
        if (updateDto.TerminationDate.HasValue)
            employee.TerminationDate = updateDto.TerminationDate;
        if (updateDto.LicenseNumber is not null)
            employee.LicenseNumber = updateDto.LicenseNumber;
        if (updateDto.LicenseExpiryDate.HasValue)
            employee.LicenseExpiryDate = updateDto.LicenseExpiryDate;
        if (updateDto.LicenseCategory is not null)
            employee.LicenseCategory = updateDto.LicenseCategory;
        if (updateDto.Salary.HasValue)
            employee.Salary = updateDto.Salary;
        if (updateDto.Notes is not null)
            employee.Notes = updateDto.Notes;
        if (updateDto.IsActive.HasValue)
            employee.IsActive = updateDto.IsActive.Value;

        await _employeeRepository.UpdateAsync(employee);
        var saved = await _employeeRepository.SaveChangesAsync();
        if (!saved)
            throw new ConflictException("SaveFailed", "Došlo je do greške prilikom ažuriranja zaposlenog.");

        _logger.LogInformation("Zaposleni ažuriran: Employee ID {EmployeeId}", employee.EmployeeId);
    }

    public async Task<bool> DeleteAsync(int employeeId)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee is null)
            throw new NotFoundException("EmployeeNotFound", $"Zaposleni sa ID {employeeId} nije pronađen.");

        await _employeeRepository.SoftDeleteAsync(employee);
        var saved = await _employeeRepository.SaveChangesAsync();

        if (saved)
            _logger.LogInformation("Zaposleni obrisan (soft delete): Employee ID {EmployeeId}", employeeId);

        return saved;
    }

    private static EmployeeReadDto MapToEmployeeReadDto(Employee employee)
    {
        return new EmployeeReadDto
        {
            EmployeeId = employee.EmployeeId,
            UserId = employee.UserId,
            Username = employee.User?.Username ?? string.Empty,
            FullName = employee.User?.FullName ?? string.Empty,
            Email = employee.User?.Email,
            Phone = employee.User?.Phone,
            EmployeeNumber = employee.EmployeeNumber,
            Position = employee.Position,
            Department = employee.Department,
            PoslovnicaId = employee.PoslovnicaId,
            PoslovnicaNaziv = employee.Poslovnica?.PJ,
            HireDate = employee.HireDate,
            TerminationDate = employee.TerminationDate,
            LicenseNumber = employee.LicenseNumber,
            LicenseExpiryDate = employee.LicenseExpiryDate,
            LicenseCategory = employee.LicenseCategory,
            Salary = employee.Salary,
            Notes = employee.Notes,
            CreatedAt = employee.CreatedAt,
            UpdatedAt = employee.UpdatedAt,
            IsActive = employee.IsActive
        };
    }
}

