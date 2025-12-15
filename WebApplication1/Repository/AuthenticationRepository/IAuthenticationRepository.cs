using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.AuthenticationRepository;
    public interface IAuthenticationRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByIdAsync(int userId);
        Task<bool> UsernameExistsAsync(string username);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
    Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
    Task<bool> SaveChangesAsync();
    }

