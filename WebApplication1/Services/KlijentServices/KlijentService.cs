using WebApplication1.Repository.KlijentRepository;
using WebApplication1.Utils.DTOs.KlijentDTO;
using WebApplication1.Utils.Exceptions;
using ValidationException = WebApplication1.Utils.Exceptions.ValidationException;

namespace WebApplication1.Services.KlijentServices;

public class KlijentService : IKlijentService
{
    private readonly IKlijentRepository _repository;

    public KlijentService(IKlijentRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<KlijentReadDto>> GetAll()
    {
        var klijenti = await _repository.GetAll().ToListAsync();
        return klijenti.Adapt<IEnumerable<KlijentReadDto>>();
    }

    public async Task<KlijentReadDto> GetById(int klijentId)
    {
        var klijent = await _repository.GetById(klijentId);
        if (klijent == null)
            throw new NotFoundException("Klijent", $"Klijent sa ID {klijentId} nije pronađen.");
        return klijent.Adapt<KlijentReadDto>();
    }

    public async Task<KlijentReadDto> Create(KlijentCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NazivFirme))
            throw new ValidationException("NazivFirme", "Naziv firme je obavezan.");

        if (string.IsNullOrWhiteSpace(dto.Drzava))
            throw new ValidationException("Drzava", "Država je obavezna.");

        if (dto.NazivFirme.Length > 200)
            throw new ValidationException("NazivFirme", "Naziv firme ne može biti duži od 200 karaktera.");

        if (dto.Drzava.Length > 100)
            throw new ValidationException("Drzava", "Država ne može biti duža od 100 karaktera.");

        var klijent = dto.Adapt<Klijent>();
        klijent.DatumKreiranja = DateTime.UtcNow;

        _repository.Create(klijent);

        if (!await _repository.SaveChangesAsync())
            throw new ConflictException("SaveFailed", "Greška prilikom kreiranja klijenta.");

        return klijent.Adapt<KlijentReadDto>();
    }

    public async Task<KlijentReadDto> Update(int klijentId, KlijentUpdateDto dto)
    {
        var klijent = await _repository.GetById(klijentId);
        if (klijent == null)
            throw new NotFoundException("Klijent", $"Klijent sa ID {klijentId} nije pronađen.");

        if (!string.IsNullOrWhiteSpace(dto.NazivFirme) && dto.NazivFirme.Length > 200)
            throw new ValidationException("NazivFirme", "Naziv firme ne može biti duži od 200 karaktera.");

        if (!string.IsNullOrWhiteSpace(dto.Drzava) && dto.Drzava.Length > 100)
            throw new ValidationException("Drzava", "Država ne može biti duža od 100 karaktera.");

        dto.Adapt(klijent);
        klijent.DatumAzuriranja = DateTime.UtcNow;

        _repository.Update(klijent);

        if (!await _repository.SaveChangesAsync())
            throw new ConflictException("SaveFailed", "Greška prilikom ažuriranja klijenta.");

        return klijent.Adapt<KlijentReadDto>();
    }

    public async Task<bool> Delete(int klijentId)
    {
        var klijent = await _repository.GetById(klijentId);
        if (klijent == null)
            throw new NotFoundException("Klijent", $"Klijent sa ID {klijentId} nije pronađen.");

        _repository.Delete(klijent);

        if (!await _repository.SaveChangesAsync())
            throw new ConflictException("DeleteFailed", "Greška prilikom brisanja klijenta.");

        return true;
    }
}

