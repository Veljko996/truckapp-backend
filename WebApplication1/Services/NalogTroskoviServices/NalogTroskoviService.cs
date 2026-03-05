using Mapster;
using WebApplication1.DataAccess.Models;
using WebApplication1.Repository.NalogRepository;
using WebApplication1.Repository.NalogTroskoviRepository;
using WebApplication1.Utils.DTOs.NalogTroskoviDTO;
using ValidationException = WebApplication1.Utils.Exceptions.ValidationException;

namespace WebApplication1.Services.NalogTroskoviServices;

public class NalogTroskoviService : INalogTroskoviService
{
    private readonly INalogTroskoviRepository _repository;
    private readonly INalogRepository _nalogRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public NalogTroskoviService(
        INalogTroskoviRepository repository,
        INalogRepository nalogRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _nalogRepository = nalogRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<NalogTrosakDto>> GetByNalogIdAsync(int nalogId)
    {
        var nalog = await _nalogRepository.GetByIdAsync(nalogId)
            ?? throw new NotFoundException("Nalog", $"Nalog sa ID {nalogId} nije pronađen.");

        ValidateInterniPrevoznik(nalog);

        var troskovi = await _repository.GetByNalogIdAsync(nalogId);
        return troskovi.Select(t => t.Adapt<NalogTrosakDto>()).ToList();
    }

    public async Task CreateAsync(int nalogId, CreateNalogTrosakDto dto)
    {
        var nalog = await _nalogRepository.GetByIdAsync(nalogId)
            ?? throw new NotFoundException("Nalog", $"Nalog sa ID {nalogId} nije pronađen.");

        ValidateInterniPrevoznik(nalog);

        var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

        var entity = dto.Adapt<NalogTrosak>();
        entity.NalogId = nalogId;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = username;

        _repository.Add(entity);
        await _repository.SaveChangesAsync();
    }

    public async Task DeleteAsync(int trosakId)
    {
        var trosak = await _repository.GetByIdAsync(trosakId)
            ?? throw new NotFoundException("NalogTrosak", $"Trošak sa ID {trosakId} nije pronađen.");

        _repository.Delete(trosak);
        await _repository.SaveChangesAsync();
    }

    public async Task<List<TipTroskaDto>> GetAllTipoviAsync()
    {
        var tipovi = await _repository.GetAllTipoviAsync();
        return tipovi.Select(t => t.Adapt<TipTroskaDto>()).ToList();
    }

    private static void ValidateInterniPrevoznik(Nalog nalog)
    {
        if (nalog.Prevoznik == null || !nalog.Prevoznik.Interni)
            throw new ValidationException("Nalog", "Troškovi se mogu unositi samo za naše vozilo (interni prevoznik).");
    }
}
