using Mapster;
using WebApplication1.DataAccess.Models;
using WebApplication1.Repository.GorivoRepository;
using WebApplication1.Repository.NasaVozilaRepository;
using WebApplication1.Repository.NalogRepository;
using WebApplication1.Utils.DTOs.GorivoDTO;
using WebApplication1.Utils.Exceptions;

namespace WebApplication1.Services.GorivoServices;

public class GorivoService : IGorivoService
{
    private readonly IGorivoRepository _repository;
    private readonly INasaVozilaRepository _voziloRepository;
    private readonly INalogRepository _nalogRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GorivoService(
        IGorivoRepository repository,
        INasaVozilaRepository voziloRepository,
        INalogRepository nalogRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _voziloRepository = voziloRepository;
        _nalogRepository = nalogRepository;
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

    public async Task CreateAsync(int voziloId, CreateGorivoZapisDto dto)
    {
        _ = await _voziloRepository.GetById(voziloId)
            ?? throw new NotFoundException("NasaVozila", $"Vozilo sa ID {voziloId} nije pronađeno.");

        if (dto.NalogId.HasValue)
        {
            _ = await _nalogRepository.GetByIdAsync(dto.NalogId.Value)
                ?? throw new NotFoundException("Nalog", $"Nalog sa ID {dto.NalogId.Value} nije pronađen.");
        }

        var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

        var entity = dto.Adapt<GorivoZapis>();
        entity.VoziloId = voziloId;
        entity.Valuta = NormalizeCurrency(entity.Valuta);
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = username;

        _repository.Add(entity);
        await _repository.SaveChangesAsync();
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
        dto.Valuta = NormalizeCurrency(entity.Valuta);
        return dto;
    }

    private static string NormalizeCurrency(string? currency)
    {
        return string.IsNullOrWhiteSpace(currency) ? "RSD" : currency.Trim().ToUpperInvariant();
    }
}
