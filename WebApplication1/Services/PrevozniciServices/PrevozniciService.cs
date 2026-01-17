using WebApplication1.Utils.DTOs.PrevoznikDTO;
using ValidationException = WebApplication1.Utils.Exceptions.ValidationException;

namespace WebApplication1.Services.PrevozniciServices;

public class PrevozniciService : IPrevozniciService
{
    private readonly IPrevozniciRepository _repository;

    public PrevozniciService(IPrevozniciRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<PrevoznikDto>> GetAll()
    {
        var prevoznici = await _repository.GetAll()
            .ToListAsync();
        
        // Don't throw exception if empty - return empty list instead
        return prevoznici.Adapt<IEnumerable<PrevoznikDto>>();
    }

    public async Task<PrevoznikDto> GetById(int prevoznikId)
    {
        var prevoznik = await _repository.GetById(prevoznikId);

        if (prevoznik == null)
            throw new NotFoundException("Prevoznik", $"Prevoznik sa ID {prevoznikId} nije pronađen.");

        return prevoznik.Adapt<PrevoznikDto>();
    }

    public async Task<PrevoznikDto> Create(CreatePrevoznikDto dto)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(dto.Naziv))
            throw new ValidationException("Naziv", "Naziv prevoznika je obavezan.");

        if (dto.Naziv.Length > 100)
            throw new ValidationException("Naziv", "Naziv prevoznika ne može biti duži od 100 karaktera.");

        // Validate optional fields if provided
        if (!string.IsNullOrWhiteSpace(dto.Kontakt) && dto.Kontakt.Length > 100)
            throw new ValidationException("Kontakt", "Kontakt ne može biti duži od 100 karaktera.");

        if (!string.IsNullOrWhiteSpace(dto.Telefon) && dto.Telefon.Length > 50)
            throw new ValidationException("Telefon", "Telefon ne može biti duži od 50 karaktera.");

        if (!string.IsNullOrWhiteSpace(dto.PIB) && dto.PIB.Length > 20)
            throw new ValidationException("PIB", "PIB ne može biti duži od 20 karaktera.");

        var prevoznik = dto.Adapt<Prevoznik>();

        _repository.Create(prevoznik);

        if (!await _repository.SaveChangesAsync())
            throw new ConflictException("SaveFailed", "Greška prilikom kreiranja prevoznika.");

        return prevoznik.Adapt<PrevoznikDto>();
    }

    public async Task<PrevoznikDto> Update(int prevoznikId, UpdatePrevoznikDto dto)
    {
        var prevoznik = await _repository.GetById(prevoznikId);

        if (prevoznik == null)
            throw new NotFoundException("PrevoznikNotFound", $"Prevoznik sa ID {prevoznikId} nije pronađen.");

        // Validate if provided
        if (!string.IsNullOrWhiteSpace(dto.Naziv) && dto.Naziv.Length > 100)
            throw new ValidationException("Naziv", "Naziv prevoznika ne može biti duži od 100 karaktera.");

        if (!string.IsNullOrWhiteSpace(dto.Kontakt) && dto.Kontakt.Length > 100)
            throw new ValidationException("Kontakt", "Kontakt ne može biti duži od 100 karaktera.");

        if (!string.IsNullOrWhiteSpace(dto.Telefon) && dto.Telefon.Length > 50)
            throw new ValidationException("Telefon", "Telefon ne može biti duži od 50 karaktera.");

        if (!string.IsNullOrWhiteSpace(dto.PIB) && dto.PIB.Length > 20)
            throw new ValidationException("PIB", "PIB ne može biti duži od 20 karaktera.");

        dto.Adapt(prevoznik);

        _repository.Update(prevoznik);

        var result = await _repository.SaveChangesAsync();
        if (!result)
            throw new ConflictException("SaveFailed", "Greška prilikom ažuriranja prevoznika.");

        return prevoznik.Adapt<PrevoznikDto>();
    }

    public async Task Delete(int prevoznikId)
    {
        var prevoznik = await _repository.GetById(prevoznikId);
        if (prevoznik == null)
            throw new NotFoundException("PrevoznikNotFound", $"Prevoznik sa ID {prevoznikId} nije pronađen.");

        // TODO: Add business rule checks if needed (e.g., check if prevoznik has active tours)
        // For now, we'll allow deletion
        
        _repository.Delete(prevoznik);

        var result = await _repository.SaveChangesAsync();
        
        if (!result)
            throw new ConflictException("DeleteFailed", "Greška prilikom brisanja prevoznika.");
    }
}

