using WebApplication1.Utils;

namespace WebApplication1.Services.AuthenticationServices;

public class AuthService : IAuthService
{
    private readonly IAuthenticationRepository _authenticationRepository;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _contextAccessor;

    public AuthService(IAuthenticationRepository authenticationRepository, IConfiguration configuration, IHttpContextAccessor contextAccessor)
    {
        _authenticationRepository = authenticationRepository;
        _configuration = configuration;
        _contextAccessor = contextAccessor;
    }

    public async Task<TokenResponseDto> LoginAsync(LoginUserDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            throw new ValidationException("EmptyUsername", "Korisničko ime je obavezno.");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length <= 4)
            throw new ValidationException("InvalidPassword", "Lozinka mora imati najmanje 4 karaktera.");

        var user = await _authenticationRepository.GetByUsernameAsync(request.Username);
        if (user is null)
            throw new NotFoundException("UserNotFound", $"Korisnik sa korisničkim imenom '{request.Username}' nije pronađen.");

        var passwordHasher = new PasswordHasher<User>();
        var passwordResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (passwordResult == PasswordVerificationResult.Failed)
        {
            // ako želiš možeš brojati pokušaje logina
            throw new ValidationException("InvalidPassword", "Pogrešna lozinka. Pokušajte ponovo.");
        }

        // Ažuriraj poslednji login
        user.LastLoginAt = DateTime.UtcNow;
        await _authenticationRepository.UpdateAsync(user);
        await _authenticationRepository.SaveChangesAsync();

        // 🔸 Sve OK, generiši token
        return await CreateTokenResponse(user);
    }


    public async Task<User> RegisterAsync(RegisterUserDto request)
    {
        // 🔹 1. Validacija unosa
        if (string.IsNullOrWhiteSpace(request.Username))
            throw new ValidationException("EmptyUsername", "Korisničko ime je obavezno.");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            throw new ValidationException("InvalidPasswordRegister", "Lozinka mora imati najmanje 6 karaktera.");

        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new ValidationException("EmptyFullName", "Ime i prezime su obavezni.");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ValidationException("EmptyEmail", "Email adresa je obavezna.");

        if (!request.Email.Contains('@'))
            throw new ValidationException("InvalidEmail", "Email adresa nije validna.");

        if (await _authenticationRepository.UsernameExistsAsync(request.Username))
            throw new ConflictException("UsernameExists", $"Korisničko ime '{request.Username}' već postoji.");

        //if (!string.IsNullOrWhiteSpace(request.Email) &&
        //    await _authenticationRepository.EmailExistsAsync(request.Email))
        //    throw new ConflictException("EmailExists", $"Email adresa '{request.Email}' već je registrovana.");

        var user = new User
        {
            Username = request.Username,
            FullName = request.FullName,
            Phone = request.Phone,
            Email = request.Email,
            RoleId = request.RoleId,
            CreatedAt = DateTime.UtcNow
        };

        var passwordHasher = new PasswordHasher<User>();
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        await _authenticationRepository.AddAsync(user);

        var result = await _authenticationRepository.SaveChangesAsync();

        if (!result)
            throw new ConflictException("SaveFailed", "Došlo je do greške prilikom kreiranja korisnika.");

        return user;
    }

    public async Task<TokenResponseDto> RefreshTokensAsync(RefreshTokenRequestDto? request = null)
    {
        var httpContext = _contextAccessor.HttpContext;
        if (httpContext is null)
            throw new ValidationException("HttpContextMissing", "HTTP context nije dostupan.");

        // Read tokens from cookies (primary method) or request body (fallback for backward compatibility)
        var accessToken = CookieHelper.GetAccessToken(httpContext.Request) 
            ?? request?.AccessToken;
        var refreshToken = CookieHelper.GetRefreshToken(httpContext.Request) 
            ?? request?.RefreshToken;

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new ValidationException("EmptyAccessToken", "Access token je obavezan.");

        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ValidationException("EmptyRefreshToken", "Refresh token je obavezan.");

        //2. Ekstrakcija korisnika iz *isteklog* Access Tokena
        var principal = JwtHelperPrincipal.GetPrincipalFromExpiredToken(accessToken, _configuration);
        if (principal is null)
            throw new ValidationException("InvalidAccessToken", "Access token nije validan.");

        var userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            throw new ValidationException("InvalidUserId", "Token ne sadrži validan identifikator korisnika.");

        //3. Proveri refresh token u bazi
        var user = await ValidateRefreshTokenAsync(userId, refreshToken);
        if (user is null)
            throw new NotFoundException("RefreshTokenInvalid", "Neispravan ili istekao refresh token.");

        // 4. Generiši novi par tokena
        var newTokens = await CreateTokenResponse(user);
        if (newTokens is null)
            throw new ConflictException("TokenGenerationFailed", "Došlo je do greške prilikom generisanja novih tokena.");

        return newTokens;
    }

    /// <summary>
    /// Invalidates refresh token for logout
    /// </summary>
    public async Task<bool> LogoutAsync(int? userId = null)
    {
        var httpContext = _contextAccessor.HttpContext;
        if (httpContext is null)
            return false;

        // If userId not provided, try to get from current user
        if (!userId.HasValue)
        {
            var userIdClaim = httpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var parsedUserId))
            {
                // No user context, just return success (cookies will be deleted by controller)
                return true;
            }
            userId = parsedUserId;
        }

        // Invalidate refresh token in database
        var user = await _authenticationRepository.GetByIdAsync(userId.Value);
        if (user is not null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _authenticationRepository.UpdateAsync(user);
            await _authenticationRepository.SaveChangesAsync();
        }

        return true;
    }


    private async Task<User?> ValidateRefreshTokenAsync(int userId, string refreshToken)
    {
        var user = await _authenticationRepository.GetByIdAsync(userId);
        if (user is null || user.RefreshToken != refreshToken
            || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return null;
        }

        return user;
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
    {
        var refreshToken = GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _authenticationRepository.UpdateAsync(user);
        await _authenticationRepository.SaveChangesAsync();
        return refreshToken;
    }

    private string CreateToken(User user)
    {
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Role, user.Roles.Name)
            };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration.GetValue<string>("AppSettings:Token")!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _configuration.GetValue<string>("AppSettings:Issuer"),
            audience: _configuration.GetValue<string>("AppSettings:Audience"),
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }

    private async Task<TokenResponseDto> CreateTokenResponse(User? user)
    {
        return new TokenResponseDto
        {
            AccessToken = CreateToken(user),
            RefreshToken = await GenerateAndSaveRefreshTokenAsync(user)
        };
    }
}