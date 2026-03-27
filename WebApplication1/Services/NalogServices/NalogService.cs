using WebApplication1.Repository.NalogRepository;
using WebApplication1.Utils.DTOs.NalogDTO;
using ValidationException = WebApplication1.Utils.Exceptions.ValidationException;

namespace WebApplication1.Services.NalogServices;

public class NalogService : INalogService
{
    private readonly INalogRepository _repository;
    private readonly ITureRepository _turaRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public NalogService(INalogRepository repository, ITureRepository turaRepository, IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _turaRepository = turaRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IEnumerable<NalogReadDto>> GetAllAsync()
    {
        return await _repository.GetAll()
            .Select(n => n.Adapt<NalogReadDto>())
            .ToListAsync();
    }

    public async Task<IEnumerable<NalogReadDto>> GetInterniAsync()
    {
        return await _repository.GetInterni()
            .Select(n => n.Adapt<NalogReadDto>())
            .ToListAsync();
    }

    public async Task<IEnumerable<NalogReadDto>> GetNaloziSaIstovaromUKasnjenjuAsync()
    {
        var list = await _repository.GetNaloziSaIstovaromUKasnjenjuAsync();
        if (list == null || list.Count == 0)
            return Array.Empty<NalogReadDto>();
        return list.Select(n => n.Adapt<NalogReadDto>()).ToList();
    }

    public async Task<NalogReadDto?> GetById(int id)
    {
        var nalog = await _repository.GetByIdAsync(id);
        if (nalog == null)
            throw new NotFoundException("Nalog", $"Nalog sa ID {id} nije pronađen.");
        return nalog.Adapt<NalogReadDto>();
    }

    public async Task<NalogReadDto> Create(int turaId, CreateNalogDto dto)
    {
        var tura = await _turaRepository.GetByIdAsync(turaId);
        if (tura == null)
            throw new NotFoundException("Tura", $"Tura sa ID {turaId} nije pronađena.");

        var nalog = await CreateNalogEntityFromTuraAsync(tura, dto.Adapt<Nalog>(), autoCreatedFromTuraAssignment: false);
        tura.StatusTure = "Kreiran Nalog";
        _repository.Add(nalog);
        await _repository.SaveChangesAsync();
        var created = await _repository.GetByIdAsync(nalog.NalogId);
        return created!.Adapt<NalogReadDto>();
    }

    public async Task<(Nalog nalog, bool created)> EnsureInternalForTuraAsync(Tura tura)
    {
        var existing = await _repository.GetActiveByTuraIdAsync(tura.TuraId);
        if (existing != null)
        {
            SyncAssignmentFieldsFromTura(existing, tura);
            _repository.Update(existing);
            return (existing, false);
        }

        var nalog = await CreateNalogEntityFromTuraAsync(
            tura,
            new Nalog(),
            autoCreatedFromTuraAssignment: true);

        _repository.Add(nalog);
        return (nalog, true);
    }

    public async Task<bool> CancelActiveInternalForTuraAsync(int turaId)
    {
        var existing = await _repository.GetActiveByTuraIdAsync(turaId);
        if (existing == null || existing.Prevoznik?.Interni != true)
            return false;

        existing.StatusNaloga = "Storniran";
        existing.FinishedAt = null;
        _repository.Update(existing);
        return true;
    }


    public async Task AssignPrevoznik(int id, AssignPrevoznikDto dto)
    {
        var nalog = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Nalog", $"Nalog sa ID {id} nije pronađen.");

        nalog.PrevoznikId = dto.PrevoznikId;

        _repository.Update(nalog);
        await _repository.SaveChangesAsync();
    }

    public async Task UpdateBusiness(int id, UpdateBusinessFieldsDto dto)
    {
        var nalog = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Nalog", $"Nalog sa ID {id} nije pronađen.");

        dto.Adapt(nalog);
        _repository.Update(nalog);
        await _repository.SaveChangesAsync();
    }
    public async Task UpdateNotes(int id, UpdateNotesDto dto)
    {
        var nalog = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Nalog", $"Nalog sa ID {id} nije pronađen.");

        dto.Adapt(nalog);
        _repository.Update(nalog);
        await _repository.SaveChangesAsync();
    }
	public async Task UpdateStatus(int id, UpdateStatusDto dto)
	{
		var nalog = await _repository.GetByIdAsync(id)
			?? throw new NotFoundException("Nalog", $"Nalog sa ID {id} nije pronađen.");

		if (string.IsNullOrWhiteSpace(dto.StatusNaloga))
			throw new ValidationException("Nalog","StatusNaloga je obavezan.");

		// Ako se postavlja Završen, faktura mora postojati
		if (dto.StatusNaloga == "Završen")
		{
			if (nalog.Istovar != true)
				throw new ValidationException("Nalog", "Nalog mora prvo biti istovaren pre završavanja.");

			if (string.IsNullOrWhiteSpace(dto.FakturaBroj))
				throw new ValidationException("Faktura","Broj fakture je obavezan za završavanje naloga.");

			nalog.FakturaBroj = dto.FakturaBroj;
		}

		// Istovaren tracking: kada status prvi put pređe na "Istovaren", zapamti timestamp
		if (dto.StatusNaloga == "Istovaren")
		{
			nalog.Istovar = true;
			nalog.IstovarenAt ??= DateTime.UtcNow;
		}

		var wasFinished = nalog.StatusNaloga == "Završen";
		var willBeFinished = dto.StatusNaloga == "Završen";

		if (!wasFinished && willBeFinished)
			nalog.FinishedAt = DateTime.UtcNow;
		else if (wasFinished && !willBeFinished)
			nalog.FinishedAt = null;

		nalog.StatusNaloga = dto.StatusNaloga;

		_repository.Update(nalog);
		await _repository.SaveChangesAsync();
	}
	public async Task MarkIstovaren(int id, MarkIstovarenDto dto)
	{
		var nalog = await _repository.GetByIdAsync(id)
			?? throw new NotFoundException("Nalog", $"Nalog sa ID {id} nije pronađen.");

		nalog.Istovar = dto.Istovar ?? true; 
		nalog.StatusNaloga = "Istovaren";
		nalog.IstovarenAt ??= DateTime.UtcNow;

		_repository.Update(nalog);
		await _repository.SaveChangesAsync();
	}

	public async Task Storniraj(int id)
    {
        var nalog = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Nalog", $"Nalog sa ID {id} nije pronađen.");

        nalog.StatusNaloga = "Storniran";
        nalog.FinishedAt = null;
        _repository.Update(nalog);
        await _repository.SaveChangesAsync();
    }
    public async Task Ponisti(int id)
    {
        var nalog = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Nalog", $"Nalog sa ID {id} nije pronađen.");

        nalog.StatusNaloga = "Ponisten";
        nalog.FinishedAt = null;
        _repository.Update(nalog);
        await _repository.SaveChangesAsync();
    }

    private async Task<Nalog> CreateNalogEntityFromTuraAsync(
        Tura tura,
        Nalog nalog,
        bool autoCreatedFromTuraAssignment)
    {
        var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
        var isInterni = IsInterniNalog(tura);
        var documentType = GetNalogDocumentType(isInterni);
        var raw = await _repository.GetNextDocumentNumberAsync(documentType);
        var nalogBroj = FormatNalogBroj(raw, isInterni);

        nalog.TuraId = tura.TuraId;
        nalog.Relacija = $"{tura.MestoUtovara} - {tura.MestoIstovara}";
        nalog.DatumUtovara = tura.DatumUtovara;
        nalog.DatumIstovara = tura.DatumIstovara;
        nalog.KolicinaRobe = tura.KolicinaRobe;
        nalog.Tezina = tura.Tezina;
        nalog.PrevoznikId = tura.PrevoznikId;
        nalog.IzvoznoCarinjenje ??= tura.IzvoznoCarinjenje;
        nalog.UvoznoCarinjenje ??= tura.UvoznoCarinjenje;
        nalog.CreatedAt = DateTime.UtcNow;
        nalog.CreatedBy = username;
        nalog.NalogBroj = nalogBroj;
        nalog.StatusNaloga = "U Toku";
        nalog.AutoCreatedFromTuraAssignment = autoCreatedFromTuraAssignment;

        SyncAssignmentFieldsFromTura(nalog, tura);
        return nalog;
    }

    private static bool IsInterniNalog(Tura tura) => tura.Prevoznik?.Interni == true;

    private static string GetNalogDocumentType(bool isInterni) => isInterni ? "NALOG_INT" : "NALOG";

    private static string FormatNalogBroj(string raw, bool isInterni)
    {
        if (!isInterni)
            return raw;

        // raw format from dbo.GetNextDocumentNumber: "{number}/{yy}" e.g. "1/26"
        var parts = raw.Split('/', 2);
        if (parts.Length != 2)
            return "I" + raw;

        if (!int.TryParse(parts[0], out var n))
            return "I" + raw;

        var yy = parts[1];
        return $"INT-{n}/{yy}";
    }

    private static void SyncAssignmentFieldsFromTura(Nalog nalog, Tura tura)
    {
        nalog.PrevoznikId = tura.PrevoznikId;

        if (tura.Prevoznik?.Interni == true && tura.VoziloId.HasValue && tura.Vozilo != null)
            nalog.RegistarskiBrojVozilaExt = tura.Vozilo.Naziv;
        else if (tura.Prevoznik?.Interni == true)
            nalog.RegistarskiBrojVozilaExt = null;
    }

}
