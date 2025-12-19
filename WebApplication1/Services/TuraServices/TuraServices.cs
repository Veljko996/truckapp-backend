namespace WebApplication1.Services.TuraServices;

public class TuraService : ITuraService
{
    private readonly ITureRepository _repository;

    public TuraService(ITureRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<TuraReadDto>> GetAll()
    {
        return await _repository.GetAll()
            .Select(t => t.Adapt<TuraReadDto>())
            .ToListAsync();
    }

    public async Task<TuraReadDto?> GetById(int id)
    {
        var tura = await _repository.GetByIdAsync(id);
        if (tura == null)
            throw new NotFoundException("Tura", $"Tura sa ID {id} nije pronađena.");

        return tura.Adapt<TuraReadDto>();
    }

    public async Task<TuraReadDto> Create(CreateTuraDto dto)
    {
        // 1. DTO -> Entity
        var tura = dto.Adapt<Tura>();

        // 2. Insert
        _repository.Create(tura);
        await _repository.SaveChangesAsync(); // sada TuraId postoji

        // 3. Generate redni broj
        int year = DateTime.UtcNow.Year % 100;
        tura.RedniBroj = $"{tura.TuraId:D2}/{year}";

        // 4. Update samo jednom
        await _repository.SaveChangesAsync();

        // 5. return
        return tura.Adapt<TuraReadDto>();
    }

    public async Task<TuraReadDto> UpdateBasic(int id, UpdateTuraDto dto)
    {
        var tura = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Tura", $"Tura sa ID {id} nije pronađena.");

        dto.Adapt(tura);
        

        _repository.Update(tura);
        await _repository.SaveChangesAsync();

        return tura.Adapt<TuraReadDto>();
    }

    public async Task<TuraReadDto> UpdateBusiness(int id, UpdateTureBusinessDto dto)
    {
        var tura = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Tura", $"Tura sa ID {id} nije pronađena.");

        dto.Adapt(tura);

        _repository.Update(tura);
        await _repository.SaveChangesAsync();

        return tura.Adapt<TuraReadDto>();
    }

    public async Task<TuraReadDto> UpdateNotes(int id, UpdateTuraNotesDto dto)
    {
        var tura = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Tura", $"Tura sa ID {id} nije pronađena.");

        dto.Adapt(tura);

        _repository.Update(tura);
        await _repository.SaveChangesAsync();

        return tura.Adapt<TuraReadDto>();
    }

    public async Task<TuraReadDto> UpdateStatus(int id, UpdateTuraStatusDto dto)
    {
        var tura = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Tura", $"Tura sa ID {id} nije pronađena.");

        dto.Adapt(tura);

        _repository.Update(tura);
        await _repository.SaveChangesAsync();

        return tura.Adapt<TuraReadDto>();
    }

    public async Task<bool> Delete(int id)
    {
        var tura = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Tura", $"Tura sa ID {id} nije pronađena.");

        _repository.Delete(tura);
        await _repository.SaveChangesAsync();

        return true;
    }


}
