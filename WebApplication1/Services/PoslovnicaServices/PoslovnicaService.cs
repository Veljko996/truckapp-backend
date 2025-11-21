using WebApplication1.DataAccess.Models;
using WebApplication1.Repository.PoslovnicaRepository;
using WebApplication1.Utils.DTOs.PoslovnicaDTO;
using WebApplication1.Utils.Exceptions;

namespace WebApplication1.Services.PoslovnicaServices;

public class PoslovnicaService : IPoslovnicaService
{
    private readonly IPoslovnicaRepository _poslovnicaRepository;
    private readonly ILogger<PoslovnicaService> _logger;

    public PoslovnicaService(IPoslovnicaRepository poslovnicaRepository, ILogger<PoslovnicaService> logger)
    {
        _poslovnicaRepository = poslovnicaRepository;
        _logger = logger;
    }

    public async Task<PoslovnicaReadDto?> GetPoslovnicaByIdAsync(int poslovnicaId)
    {
        var poslovnica = await _poslovnicaRepository.GetByIdWithEmployeesAsync(poslovnicaId);
        if (poslovnica is null)
            return null;

        return MapToPoslovnicaReadDto(poslovnica);
    }

    public async Task<IEnumerable<PoslovnicaReadDto>> GetAllPoslovniceAsync()
    {
        var poslovnice = await _poslovnicaRepository.GetAllAsync();
        return poslovnice.Select(MapToPoslovnicaReadDto);
    }

    public async Task<PoslovnicaReadDto> CreatePoslovnicaAsync(PoslovnicaCreateDto createDto)
    {
        // Validacija
        if (await _poslovnicaRepository.PJExistsAsync(createDto.PJ))
            throw new ConflictException("PJExists", $"Poslovnica sa nazivom '{createDto.PJ}' već postoji.");

        var poslovnica = new Poslovnica
        {
            PJ = createDto.PJ,
            Lokacija = createDto.Lokacija,
            BrojTelefona = createDto.BrojTelefona ?? string.Empty,
            Email = createDto.Email ?? string.Empty
        };

        await _poslovnicaRepository.AddAsync(poslovnica);
        var saved = await _poslovnicaRepository.SaveChangesAsync();

        if (!saved)
            throw new ConflictException("SaveFailed", "Došlo je do greške prilikom kreiranja poslovnice.");

        _logger.LogInformation("Poslovnica kreirana: {PJ} (ID: {PoslovnicaId})", poslovnica.PJ, poslovnica.PoslovnicaId);

        var created = await _poslovnicaRepository.GetByIdWithEmployeesAsync(poslovnica.PoslovnicaId);
        return MapToPoslovnicaReadDto(created!);
    }

    public async Task<PoslovnicaReadDto> UpdatePoslovnicaAsync(PoslovnicaUpdateDto updateDto)
    {
        var poslovnica = await _poslovnicaRepository.GetByIdAsync(updateDto.PoslovnicaId);
        if (poslovnica is null)
            throw new NotFoundException("PoslovnicaNotFound", $"Poslovnica sa ID {updateDto.PoslovnicaId} nije pronađena.");

        // Validacija PJ (ako se menja)
        if (!string.IsNullOrWhiteSpace(updateDto.PJ) && 
            await _poslovnicaRepository.PJExistsForOtherPoslovnicaAsync(updateDto.PJ, updateDto.PoslovnicaId))
            throw new ConflictException("PJExists", $"Poslovnica sa nazivom '{updateDto.PJ}' već postoji.");

        // Ažuriranje polja
        if (!string.IsNullOrWhiteSpace(updateDto.PJ))
            poslovnica.PJ = updateDto.PJ;

        if (!string.IsNullOrWhiteSpace(updateDto.Lokacija))
            poslovnica.Lokacija = updateDto.Lokacija;

        if (updateDto.BrojTelefona is not null)
            poslovnica.BrojTelefona = updateDto.BrojTelefona;

        if (updateDto.Email is not null)
            poslovnica.Email = updateDto.Email;

        await _poslovnicaRepository.UpdateAsync(poslovnica);
        var saved = await _poslovnicaRepository.SaveChangesAsync();

        if (!saved)
            throw new ConflictException("SaveFailed", "Došlo je do greške prilikom ažuriranja poslovnice.");

        _logger.LogInformation("Poslovnica ažurirana: {PJ} (ID: {PoslovnicaId})", poslovnica.PJ, poslovnica.PoslovnicaId);

        var updated = await _poslovnicaRepository.GetByIdWithEmployeesAsync(poslovnica.PoslovnicaId);
        return MapToPoslovnicaReadDto(updated!);
    }

    public async Task<bool> DeletePoslovnicaAsync(int poslovnicaId)
    {
        var poslovnica = await _poslovnicaRepository.GetByIdWithEmployeesAsync(poslovnicaId);
        if (poslovnica is null)
            throw new NotFoundException("PoslovnicaNotFound", $"Poslovnica sa ID {poslovnicaId} nije pronađena.");

        // Provera da li ima zaposlenih
        if (poslovnica.Employees.Any(e => e.IsActive))
            throw new ConflictException("PoslovnicaHasEmployees", 
                $"Poslovnica '{poslovnica.PJ}' ima aktivne zaposlene. Ne može se obrisati dok postoje aktivni zaposleni.");

        await _poslovnicaRepository.DeleteAsync(poslovnica);
        var saved = await _poslovnicaRepository.SaveChangesAsync();

        if (saved)
            _logger.LogInformation("Poslovnica obrisana: {PJ} (ID: {PoslovnicaId})", poslovnica.PJ, poslovnicaId);

        return saved;
    }

    private static PoslovnicaReadDto MapToPoslovnicaReadDto(Poslovnica poslovnica)
    {
        return new PoslovnicaReadDto
        {
            PoslovnicaId = poslovnica.PoslovnicaId,
            PJ = poslovnica.PJ,
            Lokacija = poslovnica.Lokacija,
            BrojTelefona = poslovnica.BrojTelefona,
            Email = poslovnica.Email,
            EmployeeCount = poslovnica.Employees?.Count(e => e.IsActive) ?? 0
        };
    }
}

