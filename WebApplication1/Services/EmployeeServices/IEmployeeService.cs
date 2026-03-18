using WebApplication1.Utils.DTOs.UserDTO;

namespace WebApplication1.Services.EmployeeServices;

public interface IEmployeeService
{
    Task<EmployeeReadDto?> GetByIdAsync(int employeeId);
    Task<EmployeeReadDto?> GetByUserIdAsync(int userId);
    Task<IEnumerable<EmployeeReadDto>> GetAllAsync(bool includeInactive = false);
    Task<IEnumerable<EmployeeReadDto>> GetByPoslovnicaIdAsync(int poslovnicaId);
    Task<EmployeeReadDto> CreateAsync(EmployeeCreateDto createDto);
    Task UpdateAsync(EmployeeUpdateDto updateDto);
    Task<bool> DeleteAsync(int employeeId);
}

