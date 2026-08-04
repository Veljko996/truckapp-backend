using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.AuthenticationRepository;
    public interface IAuthenticationRepository
    {
        Task<User?> GetByUsernameAndTenantAsync(string username, int tenantId);
        Task<Tenant?> GetTenantBySlugAsync(string slug);
        Task<User?> GetByIdAsync(int userId);
        Task UpdateAsync(User user);
        Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
        Task<bool> SaveChangesAsync();
    }

