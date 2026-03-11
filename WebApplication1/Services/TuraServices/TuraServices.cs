using WebApplication1.Utils.Helper;
using ValidationException = WebApplication1.Utils.Exceptions.ValidationException;
using WebApplication1.DataAccess;
using WebApplication1.Services.NalogServices;
using WebApplication1.Services.NalogPrihodiServices;

namespace WebApplication1.Services.TuraServices;

public class TuraService : ITuraService
{
    private readonly ITureRepository _repository;
    private readonly INalogService _nalogService;
    private readonly INalogPrihodiService _nalogPrihodiService;
    private readonly TruckContext _context;

    public TuraService(
        ITureRepository repository,
        INalogService nalogService,
        INalogPrihodiService nalogPrihodiService,
        TruckContext context)
    {
        _repository = repository;
        _nalogService = nalogService;
        _nalogPrihodiService = nalogPrihodiService;
        _context = context;
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

        // 2) Mapiraj DTO → Entity (samo int/int? za FK – ne string)
        var tura = dto.Adapt<Tura>();

        // 3) Sistemska polja
        tura.RedniBroj = turaBroj;
        tura.StatusTure = "Kreirana";

        // 4) Ako je VoziloId 0 (frontend ponekad šalje 0 umesto null), tretiraj kao null
        if (tura.VoziloId == 0)
            tura.VoziloId = null;

        // 5) Jedno vozilo samo na jednom aktivnom nalogu
        if (tura.VoziloId.HasValue && await _repository.IsVoziloZauzetoNaNaloguAsync(tura.VoziloId.Value, null))
            throw new ValidationException("Vozilo", "Ovo vozilo je već dodeljeno aktivnom nalogu. Jedno vozilo može biti samo na jednom nalogu.");

        // 6) Jedan insert, jedan SaveChanges
        _repository.Add(tura);
        await _repository.SaveChangesAsync();

        var created = await _repository.GetByIdAsync(tura.TuraId);
        return created!.Adapt<TuraReadDto>();
    }
	public async Task<TuraReadDto> RecreateAsync(int sourceTuraId)
	{
		var source = await _repository.GetByIdAsync(sourceTuraId);
		if (source is null)
			throw new NotFoundException("Tura", $"Tura sa ID {sourceTuraId} nije pronađena.");

		var newRedniBroj = await _repository.GetNextTuraBrojAsync();

		var newTura = new Tura
		{
			KlijentId = source.KlijentId,
			PrevoznikId = source.PrevoznikId,
			VrstaNadogradnjeId = source.VrstaNadogradnjeId,
			VoziloId = null,

			MestoUtovara = source.MestoUtovara,
			MestoIstovara = source.MestoIstovara,

			KolicinaRobe = source.KolicinaRobe,
			Tezina = source.Tezina,

			DatumUtovara = null,
			DatumIstovara = null,

			RedniBroj = newRedniBroj,
			StatusTure = "Kreirana"
		};

		_repository.Add(newTura);
		await _repository.SaveChangesAsync();

		return newTura.Adapt<TuraReadDto>();
	}

	public async Task UpdateBasic(int id, UpdateTuraDto dto)
	{
		var tura = await _repository.GetByIdAsync(id)
			?? throw new NotFoundException("Tura", $"Tura sa ID {id} nije pronađena.");

		dto.Adapt(tura);
		await _repository.SaveChangesAsync();
	}


	public async Task<UpdateTuraBusinessResultDto> UpdateBusiness(int id, UpdateTureBusinessDto dto)
	{
		var tura = await _repository.GetByIdAsync(id)
			?? throw new NotFoundException("Tura", $"Tura sa ID {id} nije pronađena.");

		dto.Adapt(tura);
		await SyncAssignmentReferencesAsync(tura);

		// Jedno vozilo samo na jednom aktivnom nalogu; pri izmeni ove ture njeno vozilo ne smatra se zauzetim
		if (tura.VoziloId.HasValue && await _repository.IsVoziloZauzetoNaNaloguAsync(tura.VoziloId.Value, id))
			throw new ValidationException("Vozilo", "Ovo vozilo je već dodeljeno drugom aktivnom nalogu. Jedno vozilo može biti samo na jednom nalogu.");

		var isInternalAssignment = tura.Prevoznik?.Interni == true;
		Nalog? nalog = null;
		var nalogCreatedNow = false;
		var seededPrihodCreatedNow = false;

		if (isInternalAssignment)
		{
			ValidateInternalAssignmentSeedData(tura);

			(nalog, nalogCreatedNow) = await _nalogService.EnsureInternalForTuraAsync(tura);
			(_, seededPrihodCreatedNow) = await _nalogPrihodiService.EnsureSeededInitialPrihodAsync(nalog, tura);
			tura.StatusTure = "Kreiran Nalog";
		}
		else
		{
			await _nalogService.CancelActiveInternalForTuraAsync(tura.TuraId);
			tura.StatusTure = "Dodeljena";
		}

		await _repository.SaveChangesAsync();

		return new UpdateTuraBusinessResultDto
		{
			TuraId = tura.TuraId,
			IsInternalAssignment = isInternalAssignment,
			NalogId = nalog?.NalogId,
			NalogCreatedNow = nalogCreatedNow,
			SeededPrihodCreatedNow = seededPrihodCreatedNow
		};
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

	private async Task SyncAssignmentReferencesAsync(Tura tura)
	{
		if (!tura.PrevoznikId.HasValue)
			throw new ValidationException("Prevoznik", "Prevoznik je obavezan.");

		tura.Prevoznik = await _context.Prevoznici.FindAsync(tura.PrevoznikId.Value)
			?? throw new ValidationException("Prevoznik", $"Prevoznik sa ID {tura.PrevoznikId.Value} ne postoji.");

		if (tura.VoziloId.HasValue)
		{
			tura.Vozilo = await _context.NasaVozila.FindAsync(tura.VoziloId.Value)
				?? throw new ValidationException("Vozilo", $"Vozilo sa ID {tura.VoziloId.Value} ne postoji.");
		}
		else
		{
			tura.Vozilo = null;
		}
	}

	private static void ValidateInternalAssignmentSeedData(Tura tura)
	{
		if (!tura.VoziloId.HasValue)
			throw new ValidationException("Vozilo", "Vozilo je obavezno za interni nalog.");

		if (!tura.IzlaznaCena.HasValue)
			throw new ValidationException("IzlaznaCena", "Izlazna cena je obavezna za interni nalog.");

		if (string.IsNullOrWhiteSpace(tura.Valuta))
			throw new ValidationException("Valuta", "Valuta je obavezna za interni nalog.");
	}


}
