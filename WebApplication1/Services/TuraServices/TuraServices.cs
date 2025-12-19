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
        var tura = dto.Adapt<Tura>();
        _repository.Add(tura);

        var saved = await _repository.SaveChangesAsync();
        if (!saved)
            throw new ConflictException("CreateFailed", "Greška prilikom kreiranja ture.");

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
