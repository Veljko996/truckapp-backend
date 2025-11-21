using Microsoft.EntityFrameworkCore;
using WebApplication1.DataAccess;
using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.UserRepository;

public class UserRepository : IUserRepository
{
    private readonly TruckContext _context;

    public UserRepository(TruckContext context)
    {
        _context = context;
    }

    // User CRUD Operations
    public async Task<User?> GetByIdAsync(int userId)
    {
        return await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<User?> GetByIdWithEmployeeAsync(int userId)
    {
        return await _context.Users
            .Include(u => u.Roles)
            .Include(u => u.Employee)
                .ThenInclude(e => e.Poslovnica)
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Roles)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetAllWithEmployeeAsync()
    {
        return await _context.Users
            .Include(u => u.Roles)
            .Include(u => u.Employee)
                .ThenInclude(e => e.Poslovnica)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _context.Users
            .Include(u => u.Roles)
            .Where(u => u.IsActive)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetByRoleIdAsync(int roleId)
    {
        return await _context.Users
            .Include(u => u.Roles)
            .Where(u => u.RoleId == roleId && u.IsActive)
            .OrderBy(u => u.FullName)
            .ToListAsync();
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _context.Users.AnyAsync(u => u.Username == username);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
            
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<bool> UsernameExistsForOtherUserAsync(string username, int excludeUserId)
    {
        return await _context.Users
            .AnyAsync(u => u.Username == username && u.UserId != excludeUserId);
    }

    public async Task<bool> EmailExistsForOtherUserAsync(string email, int excludeUserId)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
            
        return await _context.Users
            .AnyAsync(u => u.Email == email && u.UserId != excludeUserId);
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(user);
    }

    public async Task DeleteAsync(User user)
    {
        // Soft delete - samo deaktiviramo korisnika
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await UpdateAsync(user);
        
        // Ako želiš hard delete, koristi:
        // _context.Users.Remove(user);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    // Employee CRUD Operations
    public async Task<Employee?> GetEmployeeByIdAsync(int employeeId)
    {
        return await _context.Employees
            .Include(e => e.User)
                .ThenInclude(u => u.Roles)
            .Include(e => e.Poslovnica)
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
    }

    public async Task<Employee?> GetEmployeeByUserIdAsync(int userId)
    {
        return await _context.Employees
            .Include(e => e.User)
                .ThenInclude(u => u.Roles)
            .Include(e => e.Poslovnica)
            .FirstOrDefaultAsync(e => e.UserId == userId);
    }

    public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
    {
        return await _context.Employees
            .Include(e => e.User)
                .ThenInclude(u => u.Roles)
            .Include(e => e.Poslovnica)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Employee>> GetActiveEmployeesAsync()
    {
        return await _context.Employees
            .Include(e => e.User)
                .ThenInclude(u => u.Roles)
            .Include(e => e.Poslovnica)
            .Where(e => e.IsActive && e.User.IsActive)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Employee>> GetEmployeesByPoslovnicaIdAsync(int poslovnicaId)
    {
        return await _context.Employees
            .Include(e => e.User)
                .ThenInclude(u => u.Roles)
            .Include(e => e.Poslovnica)
            .Where(e => e.PoslovnicaId == poslovnicaId && e.IsActive && e.User.IsActive)
            .OrderBy(e => e.User.FullName)
            .ToListAsync();
    }

    public async Task<bool> EmployeeNumberExistsAsync(string employeeNumber)
    {
        if (string.IsNullOrWhiteSpace(employeeNumber))
            return false;
            
        return await _context.Employees.AnyAsync(e => e.EmployeeNumber == employeeNumber);
    }

    public async Task<bool> EmployeeNumberExistsForOtherEmployeeAsync(string employeeNumber, int excludeEmployeeId)
    {
        if (string.IsNullOrWhiteSpace(employeeNumber))
            return false;
            
        return await _context.Employees
            .AnyAsync(e => e.EmployeeNumber == employeeNumber && e.EmployeeId != excludeEmployeeId);
    }

    public async Task AddEmployeeAsync(Employee employee)
    {
        await _context.Employees.AddAsync(employee);
    }

    public async Task UpdateEmployeeAsync(Employee employee)
    {
        employee.UpdatedAt = DateTime.UtcNow;
        _context.Employees.Update(employee);
    }

    public async Task DeleteEmployeeAsync(Employee employee)
    {
        // Soft delete
        employee.IsActive = false;
        employee.UpdatedAt = DateTime.UtcNow;
        await UpdateEmployeeAsync(employee);
        
        // Ako želiš hard delete:
        // _context.Employees.Remove(employee);
    }
}

