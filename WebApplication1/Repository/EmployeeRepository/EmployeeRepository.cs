using Microsoft.EntityFrameworkCore;
using WebApplication1.DataAccess;
using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.EmployeeRepository;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly TruckContext _context;

    public EmployeeRepository(TruckContext context)
    {
        _context = context;
    }

    public async Task<Employee?> GetByIdAsync(int employeeId)
    {
        return await _context.Employees
            .Include(e => e.User)
                .ThenInclude(u => u.Roles)
            .Include(e => e.Poslovnica)
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
    }

    public async Task<Employee?> GetByUserIdAsync(int userId)
    {
        return await _context.Employees
            .Include(e => e.User)
                .ThenInclude(u => u.Roles)
            .Include(e => e.Poslovnica)
            .FirstOrDefaultAsync(e => e.UserId == userId);
    }

    public async Task<IEnumerable<Employee>> GetAllAsync()
    {
        return await _context.Employees
            .Include(e => e.User)
                .ThenInclude(u => u.Roles)
            .Include(e => e.Poslovnica)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Employee>> GetActiveAsync()
    {
        return await _context.Employees
            .Include(e => e.User)
                .ThenInclude(u => u.Roles)
            .Include(e => e.Poslovnica)
            .Where(e => e.IsActive && e.User.IsActive)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Employee>> GetByPoslovnicaIdAsync(int poslovnicaId)
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

    public async Task AddAsync(Employee employee)
    {
        await _context.Employees.AddAsync(employee);
    }

    public Task UpdateAsync(Employee employee)
    {
        employee.UpdatedAt = DateTime.UtcNow;
        _context.Employees.Update(employee);
        return Task.CompletedTask;
    }

    public async Task SoftDeleteAsync(Employee employee)
    {
        employee.IsActive = false;
        employee.UpdatedAt = DateTime.UtcNow;
        await UpdateAsync(employee);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}

