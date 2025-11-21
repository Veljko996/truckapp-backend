using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.UserRepository;

public interface IUserRepository
{
    // User CRUD
    Task<User?> GetByIdAsync(int userId);
    Task<User?> GetByIdWithEmployeeAsync(int userId);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
    Task<IEnumerable<User>> GetAllWithEmployeeAsync();
    Task<IEnumerable<User>> GetActiveUsersAsync();
    Task<IEnumerable<User>> GetByRoleIdAsync(int roleId);
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> UsernameExistsForOtherUserAsync(string username, int excludeUserId);
    Task<bool> EmailExistsForOtherUserAsync(string email, int excludeUserId);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);
    Task<bool> SaveChangesAsync();
    
    // Employee CRUD
    Task<Employee?> GetEmployeeByIdAsync(int employeeId);
    Task<Employee?> GetEmployeeByUserIdAsync(int userId);
    Task<IEnumerable<Employee>> GetAllEmployeesAsync();
    Task<IEnumerable<Employee>> GetActiveEmployeesAsync();
    Task<IEnumerable<Employee>> GetEmployeesByPoslovnicaIdAsync(int poslovnicaId);
    Task<bool> EmployeeNumberExistsAsync(string employeeNumber);
    Task<bool> EmployeeNumberExistsForOtherEmployeeAsync(string employeeNumber, int excludeEmployeeId);
    Task AddEmployeeAsync(Employee employee);
    Task UpdateEmployeeAsync(Employee employee);
    Task DeleteEmployeeAsync(Employee employee);
}

