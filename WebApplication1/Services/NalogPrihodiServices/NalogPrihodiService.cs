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
    private static readonly string[] PreferredCurrencies = ["EUR", "RSD"];
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

    public async Task<(NalogPrihod? prihod, bool created)> EnsureSeededInitialPrihodAsync(Nalog nalog, Tura tura)
    {
        if (!tura.IzlaznaCena.HasValue || string.IsNullOrWhiteSpace(tura.Valuta))
            return (null, false);

        var seeded = await _repository.GetSeededInitialByNalogIdAsync(nalog.NalogId);
        if (seeded != null)
        {
            if (!seeded.IsAutoSyncEnabled)
                return (seeded, false);

            seeded.Iznos = tura.IzlaznaCena.Value;
            seeded.Valuta = NormalizeCurrency(tura.Valuta);
            seeded.TipPrihoda = "Faktura";
            _repository.Update(seeded);
            return (seeded, false);
        }

        if (await _repository.HasAnyByNalogIdAsync(nalog.NalogId))
            return (null, false);

        var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
        var entity = new NalogPrihod
        {
            Nalog = nalog,
            NalogId = nalog.NalogId,
            TipPrihoda = "Faktura",
            Iznos = tura.IzlaznaCena.Value,
            Valuta = NormalizeCurrency(tura.Valuta),
            IsSeededInitial = true,
            IsAutoSyncEnabled = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = username,
        };

        _repository.Add(entity);
        return (entity, true);
    }

    public async Task<List<NalogPrihodDto>> GetByNalogIdAsync(int nalogId)
    {
        var nalog = await _nalogRepository.GetByIdAsync(nalogId)
            ?? throw new NotFoundException("Nalog", $"Nalog sa ID {nalogId} nije pronađen.");

        await ValidateNaseVoziloAsync(nalog);

        var prihodi = await _repository.GetByNalogIdAsync(nalogId);
        return prihodi
            .Select(MapPrihodDto)
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.PrihodId)
            .ToList();
    }

    public async Task CreateAsync(int nalogId, CreateNalogPrihodDto dto)
    {
        var nalog = await _nalogRepository.GetByIdAsync(nalogId)
            ?? throw new NotFoundException("Nalog", $"Nalog sa ID {nalogId} nije pronađen.");

        await ValidateNaseVoziloAsync(nalog);

        var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

        var entity = dto.Adapt<NalogPrihod>();
        entity.NalogId = nalogId;
        entity.TipPrihoda = NormalizeTipPrihoda(entity.TipPrihoda);
        entity.Valuta = NormalizeCurrency(entity.Valuta);
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
        var ukupniPrihodiPoValuti = BuildTotalsByCurrency(
            prihodi.Select(p => (Currency: (string?)p.Valuta, Amount: p.Iznos)));
        var ukupniTroskoviPoValuti = BuildTotalsByCurrency(
            troskovi.Select(t => (Currency: (string?)t.Valuta, Amount: t.Iznos)));
        // Profit po valuti = prihod - trošak (eksplicitno, da uvek bude ispravno)
        var profitPoValuti = new List<AmountByCurrencyDto>();
        var sveValute = ukupniPrihodiPoValuti.Select(x => NormalizeCurrency(x.Currency))
            .Concat(ukupniTroskoviPoValuti.Select(x => NormalizeCurrency(x.Currency)))
            .Distinct()
            .OrderBy(GetCurrencySortKey)
            .ThenBy(x => x);
        foreach (var currency in sveValute)
        {
            var prihodIznos = ukupniPrihodiPoValuti.FirstOrDefault(x => NormalizeCurrency(x.Currency) == currency)?.Amount ?? 0m;
            var trosakIznos = ukupniTroskoviPoValuti.FirstOrDefault(x => NormalizeCurrency(x.Currency) == currency)?.Amount ?? 0m;
            profitPoValuti.Add(new AmountByCurrencyDto { Currency = currency, Amount = prihodIznos - trosakIznos });
        }

        return new NalogObracunDto
        {
            NalogId = nalogId,
            Prihodi = prihodi
                .Select(MapPrihodDto)
                .OrderByDescending(p => p.CreatedAt)
                .ThenByDescending(p => p.PrihodId)
                .ToList(),
            Troskovi = troskovi
                .Select(MapTrosakDto)
                .OrderByDescending(t => t.CreatedAt)
                .ThenByDescending(t => t.TrosakId)
                .ToList(),
            UkupniPrihodiPoValuti = ukupniPrihodiPoValuti,
            UkupniTroskoviPoValuti = ukupniTroskoviPoValuti,
            ProfitPoValuti = profitPoValuti
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

    private static NalogPrihodDto MapPrihodDto(NalogPrihod prihod)
    {
        var dto = prihod.Adapt<NalogPrihodDto>();
        dto.TipPrihoda = NormalizeTipPrihoda(prihod.TipPrihoda);
        dto.Valuta = NormalizeCurrency(prihod.Valuta);
        return dto;
    }

    private static NalogTrosakDto MapTrosakDto(NalogTrosak trosak)
    {
        var dto = trosak.Adapt<NalogTrosakDto>();
        dto.Valuta = NormalizeCurrency(trosak.Valuta);
        return dto;
    }

    private static List<AmountByCurrencyDto> BuildTotalsByCurrency(IEnumerable<(string? Currency, decimal Amount)> values)
    {
        return values
            .GroupBy(x => NormalizeCurrency(x.Currency))
            .Select(g => new AmountByCurrencyDto
            {
                Currency = g.Key,
                Amount = g.Sum(x => x.Amount)
            })
            .OrderBy(GetCurrencySortKey)
            .ThenBy(x => x.Currency)
            .ToList();
    }

    private static string NormalizeTipPrihoda(string? tipPrihoda)
    {
        return string.IsNullOrWhiteSpace(tipPrihoda) ? "Ostalo" : tipPrihoda.Trim();
    }

    private static string NormalizeCurrency(string? currency)
    {
        return string.IsNullOrWhiteSpace(currency) ? "RSD" : currency.Trim().ToUpperInvariant();
    }

    private static int GetCurrencySortKey(AmountByCurrencyDto value)
    {
        return GetCurrencySortKey(value.Currency);
    }

    private static int GetCurrencySortKey(string? currency)
    {
        var normalized = NormalizeCurrency(currency);
        var index = Array.IndexOf(PreferredCurrencies, normalized);
        return index >= 0 ? index : PreferredCurrencies.Length;
    }
}
