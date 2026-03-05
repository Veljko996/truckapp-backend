using Mapster;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DataAccess;
using WebApplication1.DataAccess.Models;
using WebApplication1.Repository.NalogPrihodiRepository;
using WebApplication1.Repository.NalogRepository;
using WebApplication1.Repository.NalogTroskoviRepository;
using WebApplication1.Utils.DTOs.NalogPrihodiDTO;
using WebApplication1.Utils.DTOs.NalogTroskoviDTO;
using ValidationException = WebApplication1.Utils.Exceptions.ValidationException;

namespace WebApplication1.Services.NalogPrihodiServices;

public class NalogPrihodiService : INalogPrihodiService
{
    private readonly INalogPrihodiRepository _repository;
    private readonly INalogRepository _nalogRepository;
    private readonly INalogTroskoviRepository _troskoviRepository;
    private readonly TruckContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public NalogPrihodiService(
        INalogPrihodiRepository repository,
        INalogRepository nalogRepository,
        INalogTroskoviRepository troskoviRepository,
        TruckContext context,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _nalogRepository = nalogRepository;
        _troskoviRepository = troskoviRepository;
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<NalogPrihodDto>> GetByNalogIdAsync(int nalogId)
    {
        var nalog = await _nalogRepository.GetByIdAsync(nalogId)
            ?? throw new NotFoundException("Nalog", $"Nalog sa ID {nalogId} nije pronađen.");

        await ValidateNaseVoziloAsync(nalog);

        var prihodi = await _repository.GetByNalogIdAsync(nalogId);
        return prihodi.Select(p => p.Adapt<NalogPrihodDto>()).ToList();
    }

    public async Task CreateAsync(int nalogId, CreateNalogPrihodDto dto)
    {
        var nalog = await _nalogRepository.GetByIdAsync(nalogId)
            ?? throw new NotFoundException("Nalog", $"Nalog sa ID {nalogId} nije pronađen.");

        await ValidateNaseVoziloAsync(nalog);

        var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

        var entity = dto.Adapt<NalogPrihod>();
        entity.NalogId = nalogId;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = username;

        _repository.Add(entity);
        await _repository.SaveChangesAsync();
    }

    public async Task DeleteAsync(int prihodId)
    {
        var prihod = await _repository.GetByIdAsync(prihodId)
            ?? throw new NotFoundException("NalogPrihod", $"Prihod sa ID {prihodId} nije pronađen.");

        _repository.Delete(prihod);
        await _repository.SaveChangesAsync();
    }

    public async Task<NalogObracunDto> GetObracunAsync(int nalogId)
    {
        var nalog = await _nalogRepository.GetByIdAsync(nalogId)
            ?? throw new NotFoundException("Nalog", $"Nalog sa ID {nalogId} nije pronađen.");

        await ValidateNaseVoziloAsync(nalog);

        var prihodi = await _repository.GetByNalogIdAsync(nalogId);
        var troskovi = await _troskoviRepository.GetByNalogIdAsync(nalogId);

        var ukupniPrihodi = prihodi.Sum(p => p.Iznos);
        var ukupniTroskovi = troskovi.Sum(t => t.Iznos);

        return new NalogObracunDto
        {
            NalogId = nalogId,
            Prihodi = prihodi.Select(p => p.Adapt<NalogPrihodDto>()).ToList(),
            Troskovi = troskovi.Select(t => t.Adapt<NalogTrosakDto>()).ToList(),
            UkupniPrihodi = ukupniPrihodi,
            UkupniTroskovi = ukupniTroskovi,
            Profit = ukupniPrihodi - ukupniTroskovi
        };
    }

    private async Task ValidateNaseVoziloAsync(Nalog nalog)
    {
        var tura = await _context.Ture
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TuraId == nalog.TuraId);

        if (tura == null || tura.VoziloId == null)
            throw new ValidationException("Nalog", "Prihodi i troškovi se mogu unositi samo za naše vozilo (nalog mora imati dodeljeno naše vozilo na turi).");
    }
}
