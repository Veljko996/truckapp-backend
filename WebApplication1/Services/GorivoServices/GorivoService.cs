using Mapster;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DataAccess;
using WebApplication1.DataAccess.Models;
using WebApplication1.Repository.GorivoRepository;
using WebApplication1.Repository.KrugRepository;
using WebApplication1.Repository.NasaVozilaRepository;
using WebApplication1.Repository.NalogRepository;
using WebApplication1.Utils.DTOs.GorivoDTO;
using WebApplication1.Utils.Exceptions;
using ValidationException = WebApplication1.Utils.Exceptions.ValidationException;

namespace WebApplication1.Services.GorivoServices;

public class GorivoService : IGorivoService
{
    private readonly IGorivoRepository _repository;
    private readonly INasaVozilaRepository _voziloRepository;
    private readonly INalogRepository _nalogRepository;
    private readonly IKrugRepository _krugRepository;
    private readonly TruckContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GorivoService(
        IGorivoRepository repository,
        INasaVozilaRepository voziloRepository,
        INalogRepository nalogRepository,
        IKrugRepository krugRepository,
        TruckContext context,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _voziloRepository = voziloRepository;
        _nalogRepository = nalogRepository;
        _krugRepository = krugRepository;
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<GorivoZapisDto>> GetByVoziloIdAsync(int voziloId)
    {
        _ = await _voziloRepository.GetById(voziloId)
            ?? throw new NotFoundException("NasaVozila", $"Vozilo sa ID {voziloId} nije pronađeno.");

        var zapisi = await _repository.GetByVoziloIdAsync(voziloId);
        return zapisi.Select(MapDto).ToList();
    }

    public async Task<List<GorivoZapisDto>> GetByNalogIdAsync(int nalogId)
    {
        _ = await _nalogRepository.GetByIdAsync(nalogId)
            ?? throw new NotFoundException("Nalog", $"Nalog sa ID {nalogId} nije pronađen.");

        var zapisi = await _repository.GetByNalogIdAsync(nalogId);
        return zapisi.Select(MapDto).ToList();
    }

    public async Task<List<GorivoZapisDto>> GetByKrugIdAsync(int krugId)
    {
        _ = await _krugRepository.GetByIdAsync(krugId)
            ?? throw new NotFoundException("Krug", $"Krug sa ID {krugId} nije pronađen.");

        var zapisi = await _repository.GetByKrugIdAsync(krugId);
        return zapisi.Select(MapDto).ToList();
    }

    public async Task CreateAsync(int voziloId, CreateGorivoZapisDto dto)
    {
        var vozilo = await _voziloRepository.GetById(voziloId)
            ?? throw new NotFoundException("NasaVozila", $"Vozilo sa ID {voziloId} nije pronađeno.");

        Nalog? nalog = null;
        if (dto.NalogId.HasValue)
        {
            nalog = await _nalogRepository.GetByIdAsync(dto.NalogId.Value)
                ?? throw new NotFoundException("Nalog", $"Nalog sa ID {dto.NalogId.Value} nije pronađen.");
        }

        // KrugId pravila:
        //  1) ako je explicit prosleđen -> validiraj da pripada istom vozilu i da je otvoren
        //  2) inače pokušaj iz Tura naloga (Tura.KrugId)
        //  3) inače pokušaj iz otvorenog kruga vozila (operativno: vozač sipa, krug je otvoren)
        int? resolvedKrugId = null;

        if (dto.KrugId.HasValue)
        {
            var krug = await _krugRepository.GetByIdAsync(dto.KrugId.Value)
                ?? throw new ValidationException("KrugId", $"Krug sa ID {dto.KrugId.Value} ne postoji.");

            if (krug.VoziloId != voziloId)
                throw new ValidationException("KrugId", "Krug ne pripada izabranom vozilu.");

            if (krug.Status != "Otvoren")
                throw new ValidationException("KrugId", "Gorivo se može unositi samo u otvoren krug.");

            resolvedKrugId = krug.KrugId;
        }
        else if (nalog?.TuraId is int turaId)
        {
            var tura = await _context.Ture
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TuraId == turaId);
            resolvedKrugId = tura?.KrugId;
        }

        if (!resolvedKrugId.HasValue)
        {
            var openKrug = await _krugRepository.GetOpenByVoziloIdAsync(voziloId);
            resolvedKrugId = openKrug?.KrugId;
        }

        var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

        var entity = dto.Adapt<GorivoZapis>();
        entity.VoziloId = voziloId;
        entity.KrugId = resolvedKrugId;
        entity.Valuta = NormalizeCurrency(entity.Valuta);
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = username;

        _repository.Add(entity);
        await _repository.SaveChangesAsync();

        // Ne diramo postojeća polja vozila (kilometraža se i dalje vodi nezavisno).
        _ = vozilo;
    }

    public async Task DeleteAsync(int gorivoZapisId)
    {
        var zapis = await _repository.GetByIdAsync(gorivoZapisId)
            ?? throw new NotFoundException("GorivoZapis", $"Zapis goriva sa ID {gorivoZapisId} nije pronađen.");

        _repository.Delete(zapis);
        await _repository.SaveChangesAsync();
    }

    private static GorivoZapisDto MapDto(GorivoZapis entity)
    {
        var dto = entity.Adapt<GorivoZapisDto>();
        dto.VoziloNaziv = entity.Vozilo?.Naziv;
        dto.NalogBroj = entity.Nalog?.NalogBroj;
        dto.KrugBroj = entity.Krug?.Broj;
        dto.Valuta = NormalizeCurrency(entity.Valuta);
        return dto;
    }

    private static string NormalizeCurrency(string? currency)
    {
        return string.IsNullOrWhiteSpace(currency) ? "RSD" : currency.Trim().ToUpperInvariant();
    }
}
