using WebApplication1.Repository.UserRepository;
using ValidationException = WebApplication1.Utils.Exceptions.ValidationException;

namespace WebApplication1.Services.UserServices;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    // User Operations
    public async Task<UserReadDto?> GetUserByIdAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return null;

        return MapToUserReadDto(user);
    }

    public async Task<UserReadDto?> GetUserByIdWithEmployeeAsync(int userId)
    {
        var user = await _userRepository.GetByIdWithEmployeeAsync(userId);
        if (user is null)
            return null;

        return MapToUserReadDto(user);
    }

    public async Task<IEnumerable<UserReadDto>> GetAllUsersAsync(bool includeInactive = false)
    {
        IEnumerable<User> users;
        
        if (includeInactive)
            users = await _userRepository.GetAllAsync();
        else
            users = await _userRepository.GetActiveUsersAsync();

        return users.Select(MapToUserReadDto);
    }

    public async Task<IEnumerable<UserReadDto>> GetUsersByRoleIdAsync(int roleId)
    {
        var users = await _userRepository.GetByRoleIdAsync(roleId);
        return users.Select(MapToUserReadDto);
    }

    public async Task<UserReadDto> CreateUserAsync(UserCreateDto createDto)
    {
        // Validacija
        if (await _userRepository.UsernameExistsAsync(createDto.Username))
            throw new ConflictException("UsernameExists", $"Korisničko ime '{createDto.Username}' već postoji.");

        if (!string.IsNullOrWhiteSpace(createDto.Email) && 
            await _userRepository.EmailExistsAsync(createDto.Email))
            throw new ConflictException("EmailExists", $"Email adresa '{createDto.Email}' već postoji.");

        // Kreiranje korisnika
        var user = new User
        {
            Username = createDto.Username,
            FullName = createDto.FullName,
            Email = createDto.Email,
            Phone = createDto.Phone,
            RoleId = createDto.RoleId,
            IsActive = createDto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        // Hash lozinke
        var passwordHasher = new PasswordHasher<User>();
        user.PasswordHash = passwordHasher.HashPassword(user, createDto.Password);

        await _userRepository.AddAsync(user);
        var saved = await _userRepository.SaveChangesAsync();

        if (!saved)
            throw new ConflictException("SaveFailed", "Došlo je do greške prilikom kreiranja korisnika.");

        _logger.LogInformation("Korisnik kreiran: {Username} (ID: {UserId})", user.Username, user.UserId);

        return MapToUserReadDto(user);
    }

    public async Task UpdateUserAsync(UserUpdateDto updateDto, bool isAdmin)
    {
        if (!isAdmin && (updateDto.RoleId.HasValue || updateDto.IsActive.HasValue))
            throw new ValidationException("ForbiddenUserUpdateFields", "Nemate dozvolu za izmenu uloge ili statusa korisnika.");

        var user = await _userRepository.GetByIdAsync(updateDto.UserId);
        if (user is null)
            throw new NotFoundException("UserNotFound", $"Korisnik sa ID {updateDto.UserId} nije pronađen.");

        // Validacija username-a (ako se menja)
        // Username se ne menja u UpdateDto, ali proveravamo email

        if (!string.IsNullOrWhiteSpace(updateDto.Email) && 
            await _userRepository.EmailExistsForOtherUserAsync(updateDto.Email, updateDto.UserId))
            throw new ConflictException("EmailExists", $"Email adresa '{updateDto.Email}' već postoji.");

        // Ažuriranje polja
        if (!string.IsNullOrWhiteSpace(updateDto.FullName))
            user.FullName = updateDto.FullName;

        if (updateDto.Email is not null)
            user.Email = updateDto.Email;

        if (updateDto.Phone is not null)
            user.Phone = updateDto.Phone;

        if (updateDto.RoleId.HasValue)
            user.RoleId = updateDto.RoleId.Value;

        if (updateDto.IsActive.HasValue)
            user.IsActive = updateDto.IsActive.Value;

        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        var saved = await _userRepository.SaveChangesAsync();

        if (!saved)
            throw new ConflictException("SaveFailed", "Došlo je do greške prilikom ažuriranja korisnika.");

        _logger.LogInformation("Korisnik ažuriran: {Username} (ID: {UserId})", user.Username, user.UserId);
    }

    public async Task<bool> DeleteUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new NotFoundException("UserNotFound", $"Korisnik sa ID {userId} nije pronađen.");

        await _userRepository.DeleteAsync(user);
        var saved = await _userRepository.SaveChangesAsync();

        if (saved)
            _logger.LogInformation("Korisnik obrisan (soft delete): {Username} (ID: {UserId})", user.Username, user.UserId);

        return saved;
    }

    public async Task<bool> ChangePasswordAsync(int userId, UserChangePasswordDto changePasswordDto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new NotFoundException("UserNotFound", $"Korisnik sa ID {userId} nije pronađen.");

        // Provera trenutne lozinke
        var passwordHasher = new PasswordHasher<User>();
        var passwordResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, changePasswordDto.CurrentPassword);

        if (passwordResult == PasswordVerificationResult.Failed)
            throw new ValidationException("InvalidCurrentPassword", "Trenutna lozinka nije ispravna.");

        // Postavljanje nove lozinke
        user.PasswordHash = passwordHasher.HashPassword(user, changePasswordDto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        var saved = await _userRepository.SaveChangesAsync();

        if (saved)
            _logger.LogInformation("Lozinka promenjena za korisnika: {Username} (ID: {UserId})", user.Username, user.UserId);

        return saved;
    }

    public async Task<bool> ActivateUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new NotFoundException("UserNotFound", $"Korisnik sa ID {userId} nije pronađen.");

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        return await _userRepository.SaveChangesAsync();
    }

    public async Task<bool> DeactivateUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new NotFoundException("UserNotFound", $"Korisnik sa ID {userId} nije pronađen.");

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        return await _userRepository.SaveChangesAsync();
    }

    // Mapping methods
    private static UserReadDto MapToUserReadDto(User user)
    {
        return new UserReadDto
        {
            UserId = user.UserId,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt,
            IsActive = user.IsActive,
            RoleId = user.RoleId,
            RoleName = user.Roles?.Name ?? string.Empty,
            HasEmployeeRecord = user.Employee != null
        };
    }

}

