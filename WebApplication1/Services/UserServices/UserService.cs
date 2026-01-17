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

    public async Task<UserReadDto> UpdateUserAsync(UserUpdateDto updateDto)
    {
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

        return MapToUserReadDto(user);
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

    // Employee Operations
    public async Task<EmployeeReadDto?> GetEmployeeByIdAsync(int employeeId)
    {
        var employee = await _userRepository.GetEmployeeByIdAsync(employeeId);
        if (employee is null)
            return null;

        return MapToEmployeeReadDto(employee);
    }

    public async Task<EmployeeReadDto?> GetEmployeeByUserIdAsync(int userId)
    {
        var employee = await _userRepository.GetEmployeeByUserIdAsync(userId);
        if (employee is null)
            return null;

        return MapToEmployeeReadDto(employee);
    }

    public async Task<IEnumerable<EmployeeReadDto>> GetAllEmployeesAsync(bool includeInactive = false)
    {
        IEnumerable<Employee> employees;
        
        if (includeInactive)
            employees = await _userRepository.GetAllEmployeesAsync();
        else
            employees = await _userRepository.GetActiveEmployeesAsync();

        return employees.Select(MapToEmployeeReadDto);
    }

    public async Task<IEnumerable<EmployeeReadDto>> GetEmployeesByPoslovnicaIdAsync(int poslovnicaId)
    {
        var employees = await _userRepository.GetEmployeesByPoslovnicaIdAsync(poslovnicaId);
        return employees.Select(MapToEmployeeReadDto);
    }

    public async Task<EmployeeReadDto> CreateEmployeeAsync(EmployeeCreateDto createDto)
    {
        // Provera da li korisnik postoji
        var user = await _userRepository.GetByIdAsync(createDto.UserId);
        if (user is null)
            throw new NotFoundException("UserNotFound", $"Korisnik sa ID {createDto.UserId} nije pronađen.");

        // Provera da li korisnik već ima Employee zapis
        var existingEmployee = await _userRepository.GetEmployeeByUserIdAsync(createDto.UserId);
        if (existingEmployee is not null)
            throw new ConflictException("EmployeeExists", $"Korisnik sa ID {createDto.UserId} već ima zapis zaposlenog.");

        // Provera EmployeeNumber (ako je unet)
        if (!string.IsNullOrWhiteSpace(createDto.EmployeeNumber) &&
            await _userRepository.EmployeeNumberExistsAsync(createDto.EmployeeNumber))
            throw new ConflictException("EmployeeNumberExists", $"Broj zaposlenog '{createDto.EmployeeNumber}' već postoji.");

        var employee = new Employee
        {
            UserId = createDto.UserId,
            EmployeeNumber = createDto.EmployeeNumber,
            Position = createDto.Position,
            Department = createDto.Department,
            PoslovnicaId = createDto.PoslovnicaId,
            HireDate = createDto.HireDate ?? DateTime.UtcNow,
            LicenseNumber = createDto.LicenseNumber,
            LicenseExpiryDate = createDto.LicenseExpiryDate,
            LicenseCategory = createDto.LicenseCategory,
            Salary = createDto.Salary,
            Notes = createDto.Notes,
            IsActive = createDto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddEmployeeAsync(employee);
        var saved = await _userRepository.SaveChangesAsync();

        if (!saved)
            throw new ConflictException("SaveFailed", "Došlo je do greške prilikom kreiranja zaposlenog.");

        _logger.LogInformation("Zaposleni kreiran: User ID {UserId}, Employee ID {EmployeeId}", employee.UserId, employee.EmployeeId);

        var createdEmployee = await _userRepository.GetEmployeeByIdAsync(employee.EmployeeId);
        return MapToEmployeeReadDto(createdEmployee!);
    }

    public async Task<EmployeeReadDto> UpdateEmployeeAsync(EmployeeUpdateDto updateDto)
    {
        var employee = await _userRepository.GetEmployeeByIdAsync(updateDto.EmployeeId);
        if (employee is null)
            throw new NotFoundException("EmployeeNotFound", $"Zaposleni sa ID {updateDto.EmployeeId} nije pronađen.");

        // Provera EmployeeNumber (ako se menja)
        if (!string.IsNullOrWhiteSpace(updateDto.EmployeeNumber) &&
            await _userRepository.EmployeeNumberExistsForOtherEmployeeAsync(updateDto.EmployeeNumber, updateDto.EmployeeId))
            throw new ConflictException("EmployeeNumberExists", $"Broj zaposlenog '{updateDto.EmployeeNumber}' već postoji.");

        // Ažuriranje polja
        if (updateDto.EmployeeNumber is not null)
            employee.EmployeeNumber = updateDto.EmployeeNumber;

        if (updateDto.Position is not null)
            employee.Position = updateDto.Position;

        if (updateDto.Department is not null)
            employee.Department = updateDto.Department;

        if (updateDto.PoslovnicaId.HasValue)
            employee.PoslovnicaId = updateDto.PoslovnicaId;

        if (updateDto.HireDate.HasValue)
            employee.HireDate = updateDto.HireDate;

        if (updateDto.TerminationDate.HasValue)
            employee.TerminationDate = updateDto.TerminationDate;

        if (updateDto.LicenseNumber is not null)
            employee.LicenseNumber = updateDto.LicenseNumber;

        if (updateDto.LicenseExpiryDate.HasValue)
            employee.LicenseExpiryDate = updateDto.LicenseExpiryDate;

        if (updateDto.LicenseCategory is not null)
            employee.LicenseCategory = updateDto.LicenseCategory;

        if (updateDto.Salary.HasValue)
            employee.Salary = updateDto.Salary;

        if (updateDto.Notes is not null)
            employee.Notes = updateDto.Notes;

        if (updateDto.IsActive.HasValue)
            employee.IsActive = updateDto.IsActive.Value;

        employee.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateEmployeeAsync(employee);
        var saved = await _userRepository.SaveChangesAsync();

        if (!saved)
            throw new ConflictException("SaveFailed", "Došlo je do greške prilikom ažuriranja zaposlenog.");

        _logger.LogInformation("Zaposleni ažuriran: Employee ID {EmployeeId}", employee.EmployeeId);

        var updatedEmployee = await _userRepository.GetEmployeeByIdAsync(employee.EmployeeId);
        return MapToEmployeeReadDto(updatedEmployee!);
    }

    public async Task<bool> DeleteEmployeeAsync(int employeeId)
    {
        var employee = await _userRepository.GetEmployeeByIdAsync(employeeId);
        if (employee is null)
            throw new NotFoundException("EmployeeNotFound", $"Zaposleni sa ID {employeeId} nije pronađen.");

        await _userRepository.DeleteEmployeeAsync(employee);
        var saved = await _userRepository.SaveChangesAsync();

        if (saved)
            _logger.LogInformation("Zaposleni obrisan (soft delete): Employee ID {EmployeeId}", employeeId);

        return saved;
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

    private static EmployeeReadDto MapToEmployeeReadDto(Employee employee)
    {
        return new EmployeeReadDto
        {
            EmployeeId = employee.EmployeeId,
            UserId = employee.UserId,
            Username = employee.User?.Username ?? string.Empty,
            FullName = employee.User?.FullName ?? string.Empty,
            Email = employee.User?.Email,
            Phone = employee.User?.Phone,
            EmployeeNumber = employee.EmployeeNumber,
            Position = employee.Position,
            Department = employee.Department,
            PoslovnicaId = employee.PoslovnicaId,
            PoslovnicaNaziv = employee.Poslovnica?.PJ,
            HireDate = employee.HireDate,
            TerminationDate = employee.TerminationDate,
            LicenseNumber = employee.LicenseNumber,
            LicenseExpiryDate = employee.LicenseExpiryDate,
            LicenseCategory = employee.LicenseCategory,
            Salary = employee.Salary,
            Notes = employee.Notes,
            CreatedAt = employee.CreatedAt,
            UpdatedAt = employee.UpdatedAt,
            IsActive = employee.IsActive
        };
    }
}

