using WebApplication1.Utils.Helper;

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
        // 1) Uzmi sledeći broj iz baze (DB broji)
        var turaBroj = await _repository.GetNextTuraBrojAsync();

        // 2) Mapiraj DTO → Entity
        var tura = dto.Adapt<Tura>();

        // 3) Sistemsка polja
        tura.RedniBroj = turaBroj;
        tura.StatusTure = "Kreirana"; 
        
        // 4) Jedan insert, jedan SaveChanges
        _repository.Add(tura);
        await _repository.SaveChangesAsync();

        // 5) Re-query ako ti treba kompletan graf
        var created = await _repository.GetByIdAsync(tura.TuraId);
        return created!.Adapt<TuraReadDto>();
    }


	public async Task UpdateBasic(int id, UpdateTuraDto dto)
	{
		var tura = await _repository.GetByIdAsync(id)
			?? throw new NotFoundException("Tura", $"Tura sa ID {id} nije pronađena.");

		dto.Adapt(tura);
		await _repository.SaveChangesAsync();
	}


	public async Task UpdateBusiness(int id, UpdateTureBusinessDto dto)
	{
		var tura = await _repository.GetByIdAsync(id)
			?? throw new NotFoundException("Tura", $"Tura sa ID {id} nije pronađena.");

		dto.Adapt(tura);
		tura.StatusTure = "Dodeljena";

		await _repository.SaveChangesAsync();
	}


	public async Task UpdateNotes(int id, UpdateTuraNotesDto dto)
	{
		var tura = await _repository.GetByIdAsync(id)
			?? throw new NotFoundException("Tura", $"Tura sa ID {id} nije pronađena.");

		dto.Adapt(tura);
		await _repository.SaveChangesAsync();
	}


	public async Task UpdateStatus(int id, UpdateTuraStatusDto dto)
	{
		var tura = await _repository.GetByIdAsync(id)
			?? throw new NotFoundException("Tura", $"Tura sa ID {id} nije pronađena.");

		dto.Adapt(tura);
		await _repository.SaveChangesAsync();
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
