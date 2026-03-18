namespace WebApplication1.Repository.EmployeeRepository;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(int employeeId);
    Task<Employee?> GetByUserIdAsync(int userId);
    Task<IEnumerable<Employee>> GetAllAsync();
    Task<IEnumerable<Employee>> GetActiveAsync();
    Task<IEnumerable<Employee>> GetByPoslovnicaIdAsync(int poslovnicaId);

    Task<bool> EmployeeNumberExistsAsync(string employeeNumber);
    Task<bool> EmployeeNumberExistsForOtherEmployeeAsync(string employeeNumber, int excludeEmployeeId);

    Task AddAsync(Employee employee);
    Task UpdateAsync(Employee employee);
    Task SoftDeleteAsync(Employee employee);
    Task<bool> SaveChangesAsync();
}

