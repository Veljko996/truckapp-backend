using Mapster;
using WebApplication1.DataAccess.Models;
using WebApplication1.Repository.KrugRepository;
using WebApplication1.Repository.KrugTroskoviRepository;
using WebApplication1.Utils.DTOs.KrugTroskoviDTO;
using WebApplication1.Utils.Exceptions;
using ValidationException = WebApplication1.Utils.Exceptions.ValidationException;

namespace WebApplication1.Services.KrugTroskoviServices;

public class KrugTroskoviService : IKrugTroskoviService
{
    private readonly IKrugTroskoviRepository _repository;
    private readonly IKrugRepository _krugRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public KrugTroskoviService(
        IKrugTroskoviRepository repository,
        IKrugRepository krugRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _krugRepository = krugRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<KrugTrosakDto>> GetByKrugIdAsync(int krugId)
    {
        var krug = await _krugRepository.GetByIdAsync(krugId)
            ?? throw new NotFoundException("Krug", $"Krug sa ID {krugId} nije pronađen.");

        var troskovi = await _repository.GetByKrugIdAsync(krug.KrugId);
        return troskovi.Select(MapDto).ToList();
    }

    public async Task CreateAsync(int krugId, CreateKrugTrosakDto dto)
    {
        var krug = await _krugRepository.GetByIdAsync(krugId)
            ?? throw new NotFoundException("Krug", $"Krug sa ID {krugId} nije pronađen.");

        if (krug.Status != "Otvoren")
            throw new ValidationException("Krug", "Trošak se može dodati samo u otvoren krug.");

        var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

        var entity = dto.Adapt<KrugTrosak>();
        entity.KrugId = krug.KrugId;
        entity.Valuta = NormalizeCurrency(entity.Valuta);
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = username;

        _repository.Add(entity);
        await _repository.SaveChangesAsync();
    }

    public async Task DeleteAsync(int krugTrosakId)
    {
        var trosak = await _repository.GetByIdAsync(krugTrosakId)
            ?? throw new NotFoundException("KrugTrosak", $"Trošak sa ID {krugTrosakId} nije pronađen.");

        var krug = await _krugRepository.GetByIdAsync(trosak.KrugId);
        if (krug != null && krug.Status != "Otvoren")
            throw new ValidationException("Krug", "Trošak se može obrisati samo u otvorenom krugu.");

        _repository.Delete(trosak);
        await _repository.SaveChangesAsync();
    }

    private static KrugTrosakDto MapDto(KrugTrosak t)
    {
        return new KrugTrosakDto
        {
            KrugTrosakId = t.KrugTrosakId,
            KrugId = t.KrugId,
            TipTroskaId = t.TipTroskaId,
            TipNaziv = t.TipTroska?.Naziv,
            Iznos = t.Iznos,
            Valuta = NormalizeCurrency(t.Valuta),
            Napomena = t.Napomena,
            CreatedAt = t.CreatedAt,
            CreatedBy = t.CreatedBy
        };
    }

    private static string NormalizeCurrency(string? currency)
    {
        return string.IsNullOrWhiteSpace(currency) ? "RSD" : currency.Trim().ToUpperInvariant();
    }
}
