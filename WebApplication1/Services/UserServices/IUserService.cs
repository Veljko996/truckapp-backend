using WebApplication1.Utils.DTOs.UserDTO;

namespace WebApplication1.Services.UserServices;

public interface IUserService
{
    // User operations
    Task<UserReadDto?> GetUserByIdAsync(int userId);
    Task<UserReadDto?> GetUserByIdWithEmployeeAsync(int userId);
    Task<IEnumerable<UserReadDto>> GetAllUsersAsync(bool includeInactive = false);
    Task<IEnumerable<UserReadDto>> GetUsersByRoleIdAsync(int roleId);
    Task<UserReadDto> CreateUserAsync(UserCreateDto createDto);
    Task<UserReadDto> UpdateUserAsync(UserUpdateDto updateDto);
    Task<bool> DeleteUserAsync(int userId);
    Task<bool> ChangePasswordAsync(int userId, UserChangePasswordDto changePasswordDto);
    Task<bool> ActivateUserAsync(int userId);
    Task<bool> DeactivateUserAsync(int userId);
    
    // Employee operations
    Task<EmployeeReadDto?> GetEmployeeByIdAsync(int employeeId);
    Task<EmployeeReadDto?> GetEmployeeByUserIdAsync(int userId);
    Task<IEnumerable<EmployeeReadDto>> GetAllEmployeesAsync(bool includeInactive = false);
    Task<IEnumerable<EmployeeReadDto>> GetEmployeesByPoslovnicaIdAsync(int poslovnicaId);
    Task<EmployeeReadDto> CreateEmployeeAsync(EmployeeCreateDto createDto);
    Task<EmployeeReadDto> UpdateEmployeeAsync(EmployeeUpdateDto updateDto);
    Task<bool> DeleteEmployeeAsync(int employeeId);
}

