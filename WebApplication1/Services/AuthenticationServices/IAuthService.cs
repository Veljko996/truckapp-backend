namespace WebApplication1.Services.AuthenticationServices;

    public interface IAuthService
    {
        Task<LoginResultDto?> LoginAsync(LoginUserDto request);
        Task<User?> RegisterAsync(RegisterUserDto request);
        Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto? request = null);
        Task<bool> LogoutAsync(int? userId = null);
        Task ChangePasswordAsync(int userId, ChangePasswordDto request);
        Task AdminResetPasswordAsync(int userId, AdminResetPasswordDto request);
        Task<User?> GetUserByIdForMeAsync(int userId);
    }