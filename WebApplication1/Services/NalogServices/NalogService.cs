using WebApplication1.Repository.NalogRepository;
using WebApplication1.Utils.DTOs.NalogDTO;

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

        var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

        var nalog = dto.Adapt<Nalog>();

        nalog.TuraId = turaId;
        nalog.Relacija = $"{tura.MestoUtovara} - {tura.MestoIstovara}";
        nalog.DatumUtovara = tura.DatumUtovaraOd; 
        nalog.DatumIstovara = tura.DatumIstovaraDo;
        nalog.KolicinaRobe = tura.KolicinaRobe;

        //polja koja se vuku iz ture ali se mogu menjati na nalogu
        nalog.PrevoznikId = tura.PrevoznikId;
        nalog.IzvoznoCarinjenje = tura.IzvoznoCarinjenje;
        nalog.UvoznoCarinjenje = tura.UvoznoCarinjenje;
        nalog.CreatedAt = DateTime.UtcNow;
        nalog.CreatedBy = username;

        _repository.Add(nalog);
        await _repository.SaveChangesAsync();

        nalog.NalogBroj = $"{nalog.NalogId}/{DateTime.Now.Year % 100}";
        nalog.StatusNaloga = "U Toku";
        tura.StatusTure = "Kreiran Nalog";

        await _repository.SaveChangesAsync();

        // Re-query to load navigation properties
        var created = await _repository.GetByIdAsync(nalog.NalogId);
        return created!.Adapt<NalogReadDto>();
    }

    public async Task<NalogReadDto> AssignPrevoznik(int id, AssignPrevoznikDto dto)
    {
        var nalog = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Nalog", $"Nalog sa ID {id} nije pronađen.");

        nalog.PrevoznikId = dto.PrevoznikId;
        
        _repository.Update(nalog);
        await _repository.SaveChangesAsync();

        // Re-query to load navigation properties
        var updated = await _repository.GetByIdAsync(id);
        return updated!.Adapt<NalogReadDto>();
    }

    public async Task<NalogReadDto> UpdateBusiness(int id, UpdateBusinessFieldsDto dto)
    {
        var nalog = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Nalog", $"Nalog sa ID {id} nije pronađen.");
        
        dto.Adapt(nalog);
        _repository.Update(nalog);
        await _repository.SaveChangesAsync();

        // Re-query to load navigation properties
        var updated = await _repository.GetByIdAsync(id);
        return updated!.Adapt<NalogReadDto>();
    }
    public async Task<NalogReadDto> UpdateNotes(int id, UpdateNotesDto dto)
    {
        var nalog = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Nalog", $"Nalog sa ID {id} nije pronađen.");
        
        dto.Adapt(nalog);
        _repository.Update(nalog);
        await _repository.SaveChangesAsync();

        // Re-query to load navigation properties
        var updated = await _repository.GetByIdAsync(id);
        return updated!.Adapt<NalogReadDto>();
    }
    public async Task<NalogReadDto> UpdateStatus(int id, UpdateStatusDto dto)
    {
        var nalog = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Nalog", $"Nalog sa ID {id} nije pronađen.");
        
        dto.Adapt(nalog);
        _repository.Update(nalog);
        await _repository.SaveChangesAsync();

        // Re-query to load navigation properties
        var updated = await _repository.GetByIdAsync(id);
        return updated!.Adapt<NalogReadDto>();
    }
    public async Task<NalogReadDto> MarkIstovaren(int id, MarkIstovarenDto dto)
    {
        var nalog = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Nalog", $"Nalog sa ID {id} nije pronađen.");
        
        dto.Adapt(nalog);
        _repository.Update(nalog);
        await _repository.SaveChangesAsync();

        // Re-query to load navigation properties
        var updated = await _repository.GetByIdAsync(id);
        return updated!.Adapt<NalogReadDto>();
    }

    public async Task<NalogReadDto> Storniraj(int id)
    {
        var nalog = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Nalog", $"Nalog sa ID {id} nije pronađen.");
        
        nalog.StatusNaloga = "Storniran";
        _repository.Update(nalog);
        await _repository.SaveChangesAsync();

        // Re-query to load navigation properties
        var updated = await _repository.GetByIdAsync(id);
        return updated!.Adapt<NalogReadDto>();
    }
    public async Task<NalogReadDto> Ponisti(int id)
    {
        var nalog = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Nalog", $"Nalog sa ID {id} nije pronađen.");
        
        nalog.StatusNaloga = "Ponisten";
        _repository.Update(nalog);
        await _repository.SaveChangesAsync();

        // Re-query to load navigation properties
        var updated = await _repository.GetByIdAsync(id);
        return updated!.Adapt<NalogReadDto>();
    }


}
