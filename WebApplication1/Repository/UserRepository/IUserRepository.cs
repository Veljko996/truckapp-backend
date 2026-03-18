
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
}

