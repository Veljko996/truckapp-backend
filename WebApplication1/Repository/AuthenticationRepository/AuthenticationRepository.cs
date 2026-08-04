namespace WebApplication1.Repository.AuthenticationRepository;

public class AuthenticationRepository : IAuthenticationRepository
{
    private readonly TruckContext _context;

    public AuthenticationRepository(TruckContext context)
    {
        _context = context;
    }
    public async Task<User?> GetByUsernameAndTenantAsync(string username, int tenantId)
    {
        return await _context.Users
            .IgnoreQueryFilters()
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Username == username && u.TenantId == tenantId);
    }

    public async Task<Tenant?> GetTenantBySlugAsync(string slug)
    {
        return await _context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Slug == slug && t.IsActive);
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        return await _context
            .Users
            .IgnoreQueryFilters()
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task UpdateAsync(User user)
    {
       _context.Users.Update(user);
    }
    public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
    {
        var now = DateTime.UtcNow;
        // Matchuj tekući token (ako nije istekao) ILI prethodni u grace prozoru.
        return await _context.Users
            .IgnoreQueryFilters()
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u =>
                (u.RefreshToken == refreshToken && u.RefreshTokenExpiryTime > now) ||
                (u.PreviousRefreshToken == refreshToken && u.PreviousRefreshTokenExpiryTime > now));
    }


    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }


}
