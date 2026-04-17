using Mapster;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DataAccess;
using WebApplication1.DataAccess.Models;
using WebApplication1.Repository.KrugRepository;
using WebApplication1.Repository.KrugTroskoviRepository;
using WebApplication1.Repository.NalogRepository;
using WebApplication1.Services.NalogServices;
using WebApplication1.Services.NalogPrihodiServices;
using WebApplication1.Services.NalogVozacAccessServices;
using WebApplication1.Services.TuraServices;
using WebApplication1.Utils.DTOs.KrugDTO;
using WebApplication1.Utils.DTOs.KrugTroskoviDTO;
using WebApplication1.Utils.DTOs.NalogDTO;
using WebApplication1.Utils.DTOs.NalogPrihodiDTO;
using WebApplication1.Utils.Exceptions;
using ValidationException = WebApplication1.Utils.Exceptions.ValidationException;

namespace WebApplication1.Services.KrugServices;

public class KrugService : IKrugService
{
    private readonly IKrugRepository _repository;
    private readonly IKrugTroskoviRepository _troskoviRepository;
    private readonly INalogRepository _nalogRepository;
    private readonly INalogService _nalogService;
    private readonly INalogPrihodiService _nalogPrihodiService;
    private readonly ITuraService _turaService;
    private readonly ITureRepository _turaRepository;
    private readonly INalogVozacAccessService _vozacAccess;
    private readonly TruckContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public KrugService(
        IKrugRepository repository,
        IKrugTroskoviRepository troskoviRepository,
        INalogRepository nalogRepository,
        INalogService nalogService,
        INalogPrihodiService nalogPrihodiService,
        ITuraService turaService,
        ITureRepository turaRepository,
        INalogVozacAccessService vozacAccess,
        TruckContext context,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _troskoviRepository = troskoviRepository;
        _nalogRepository = nalogRepository;
        _nalogService = nalogService;
        _nalogPrihodiService = nalogPrihodiService;
        _turaService = turaService;
        _turaRepository = turaRepository;
        _vozacAccess = vozacAccess;
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<KrugReadDto>> GetAllAsync(int? vozacUserId = null)
    {
        var query = _repository.GetAll();

        if (vozacUserId.HasValue)
        {
            // Vozac vidi samo krugove vozila na koje je trenutno dodeljen.
            var userId = vozacUserId.Value;
            query = query.Where(k =>
                _context.NasaVoziloVozacAssignments.Any(a =>
                    a.VoziloId == k.VoziloId &&
                    a.UnassignedAt == null &&
                    a.Employee!.UserId == userId));
        }

        var krugovi = await query.ToListAsync();
        return krugovi.Select(MapReadDto).ToList();
    }

    public async Task<KrugDetailsDto> GetDetailsAsync(int krugId, int? vozacUserId = null)
    {
        var krug = await _repository.GetByIdWithTureAsync(krugId)
            ?? throw new NotFoundException("Krug", $"Krug sa ID {krugId} nije pronađen.");

        if (vozacUserId.HasValue &&
            !await _vozacAccess.CanAccessVoziloAsync(vozacUserId.Value, krug.VoziloId))
        {
            // Vozac nema pristup vozilu ovog kruga -> nek izgleda kao "ne postoji"
            throw new NotFoundException("Krug", $"Krug sa ID {krugId} nije pronađen.");
        }

        var tureIds = krug.Ture.Select(t => t.TuraId).ToList();

        // Učitaj naloge vezane za sve ture u krugu (nije Storniran/Ponisten)
        var nalozi = await _context.Nalozi
            .AsNoTracking()
            .Include(n => n.Prevoznik)
            .Include(n => n.Tura)!
                .ThenInclude(t => t!.Vozilo)
            .Include(n => n.Troskovi)
            .Include(n => n.Prihodi)
            .Where(n => tureIds.Contains(n.TuraId)
                && n.StatusNaloga != "Storniran"
                && n.StatusNaloga != "Ponisten")
            .ToListAsync();

        var nalogPoTuri = nalozi.GroupBy(n => n.TuraId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(n => n.NalogId).First());

        var details = new KrugDetailsDto
        {
            KrugId = krug.KrugId,
            Broj = krug.Broj,
            VoziloId = krug.VoziloId,
            VoziloNaziv = krug.Vozilo?.Naziv,
            StartAt = krug.StartAt,
            EndAt = krug.EndAt,
            Status = krug.Status,
            Napomena = krug.Napomena,
            CreatedAt = krug.CreatedAt,
            CreatedBy = krug.CreatedBy,
            ClosedAt = krug.ClosedAt,
            ClosedBy = krug.ClosedBy,
            Troskovi = krug.Troskovi
                .OrderByDescending(t => t.CreatedAt)
                .Select(MapKrugTrosakDto)
                .ToList(),
            Ture = krug.Ture
                .OrderByDescending(t => t.TuraId)
                .Select(t => new KrugTuraItemDto
                {
                    TuraId = t.TuraId,
                    RedniBroj = t.RedniBroj,
                    MestoUtovara = t.MestoUtovara,
                    MestoIstovara = t.MestoIstovara,
                    DatumUtovara = t.DatumUtovara,
                    DatumIstovara = t.DatumIstovara,
                    StatusTure = t.StatusTure,
                    KlijentNaziv = t.Klijent?.NazivFirme,
                    PrevoznikNaziv = t.Prevoznik?.Naziv,
                    PrevoznikInterni = t.Prevoznik?.Interni,
                    Nalog = nalogPoTuri.TryGetValue(t.TuraId, out var n) ? n.Adapt<NalogReadDto>() : null
                })
                .ToList()
        };

        // Finansijski rezime preko zajedničkog helpera
        var (troskoviKruga, troskoviNaloga, prihodi, profit) = BuildFinancialSummary(krug, nalozi);
        details.UkupniTroskoviKrugaPoValuti = troskoviKruga;
        details.UkupniTroskoviNalogaPoValuti = troskoviNaloga;
        details.UkupniPrihodiPoValuti = prihodi;
        details.ProfitPoValuti = profit;

        return details;
    }

    public async Task<KrugFinancialSummaryDto> GetFinancialSummaryAsync(int krugId, int? vozacUserId = null)
    {
        var krug = await _repository.GetByIdWithTureAsync(krugId)
            ?? throw new NotFoundException("Krug", $"Krug sa ID {krugId} nije pronađen.");

        if (vozacUserId.HasValue &&
            !await _vozacAccess.CanAccessVoziloAsync(vozacUserId.Value, krug.VoziloId))
        {
            throw new NotFoundException("Krug", $"Krug sa ID {krugId} nije pronađen.");
        }

        var tureIds = krug.Ture.Select(t => t.TuraId).ToList();

        var nalozi = await _context.Nalozi
            .AsNoTracking()
            .Include(n => n.Troskovi)
            .Include(n => n.Prihodi)
            .Where(n => tureIds.Contains(n.TuraId)
                && n.StatusNaloga != "Storniran"
                && n.StatusNaloga != "Ponisten")
            .ToListAsync();

        var (troskoviKruga, troskoviNaloga, prihodi, profit) = BuildFinancialSummary(krug, nalozi);

        return new KrugFinancialSummaryDto
        {
            KrugId = krug.KrugId,
            Broj = krug.Broj,
            Status = krug.Status,
            BrojTura = krug.Ture.Count,
            BrojNaloga = nalozi.Count,
            UkupniTroskoviKrugaPoValuti = troskoviKruga,
            UkupniTroskoviNalogaPoValuti = troskoviNaloga,
            UkupniPrihodiPoValuti = prihodi,
            ProfitPoValuti = profit
        };
    }

    public async Task<KrugReadDto?> GetOpenByVoziloAsync(int voziloId, int? vozacUserId = null)
    {
        if (vozacUserId.HasValue &&
            !await _vozacAccess.CanAccessVoziloAsync(vozacUserId.Value, voziloId))
        {
            return null;
        }

        var open = await _repository.GetOpenByVoziloIdAsync(voziloId);
        if (open == null) return null;

        // Load vozilo + ture za BrojTura
        var full = await _repository.GetByIdWithTureAsync(open.KrugId);
        return full != null ? MapReadDto(full) : MapReadDto(open);
    }

    public async Task<KrugReadDto> CreateAsync(CreateKrugDto dto)
    {
        var vozilo = await _context.NasaVozila.FindAsync(dto.VoziloId)
            ?? throw new ValidationException("Vozilo", $"Vozilo sa ID {dto.VoziloId} ne postoji.");

        var existingOpen = await _repository.GetOpenByVoziloIdAsync(dto.VoziloId);
        if (existingOpen != null)
            throw new ConflictException("Krug", $"Vozilo '{vozilo.Naziv}' već ima otvoren krug (#{existingOpen.KrugId}).");

        var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
        var nextBroj = await _repository.GetNextKrugBrojAsync();

        var krug = new Krug
        {
            Broj = FormatKrugBroj(nextBroj),
            VoziloId = dto.VoziloId,
            StartAt = dto.StartAt ?? DateTime.UtcNow,
            Status = "Otvoren",
            Napomena = dto.Napomena,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = username
        };

        _repository.Add(krug);
        await _repository.SaveChangesAsync();

        var created = await _repository.GetByIdAsync(krug.KrugId);
        return MapReadDto(created!);
    }

    public async Task<KrugReadDto> CreateFromNalogAsync(int nalogId)
    {
        var nalog = await _nalogRepository.GetByIdAsync(nalogId)
            ?? throw new NotFoundException("Nalog", $"Nalog sa ID {nalogId} nije pronađen.");

        if (nalog.Tura == null)
            throw new ValidationException("Nalog", "Nalog nema povezanu turu.");

        var voziloId = nalog.Tura.VoziloId;
        if (!voziloId.HasValue)
            throw new ValidationException("Vozilo", "Tura ovog naloga nema dodeljeno vozilo. Krug ne može biti kreiran.");

        if (nalog.Tura.KrugId.HasValue)
            throw new ConflictException("Krug", $"Tura ovog naloga je već u krugu (#{nalog.Tura.KrugId.Value}).");

        var existingOpen = await _repository.GetOpenByVoziloIdAsync(voziloId.Value);
        if (existingOpen != null)
            throw new ConflictException("Krug", $"Vozilo već ima otvoren krug (#{existingOpen.KrugId}).");

        var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
        var nextBroj = await _repository.GetNextKrugBrojAsync();

        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            var krug = new Krug
            {
                Broj = FormatKrugBroj(nextBroj),
                VoziloId = voziloId.Value,
                StartAt = DateTime.UtcNow,
                Status = "Otvoren",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = username
            };

            _repository.Add(krug);
            await _repository.SaveChangesAsync();

            // Poveži postojeću Turu sa novim Krugom
            await _turaService.AssignKrugAsync(nalog.TuraId, krug.KrugId);

            await tx.CommitAsync();

            var created = await _repository.GetByIdAsync(krug.KrugId);
            return MapReadDto(created!);
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<NalogReadDto> CreateNalogForKrugAsync(int krugId, CreateNalogForKrugDto dto)
    {
        var krug = await _repository.GetByIdAsync(krugId)
            ?? throw new NotFoundException("Krug", $"Krug sa ID {krugId} nije pronađen.");

        if (krug.Status != "Otvoren")
            throw new ValidationException("Krug", "Nije moguće dodati nalog u zatvoren krug.");

        // Validacija: prevoznik mora biti interni (pošto pravimo interni nalog za vozilo kruga)
        var prevoznik = await _context.Prevoznici.FindAsync(dto.PrevoznikId)
            ?? throw new ValidationException("Prevoznik", $"Prevoznik sa ID {dto.PrevoznikId} ne postoji.");

        if (!prevoznik.Interni)
            throw new ValidationException("Prevoznik", "Za nalog u krugu mora biti izabran interni prevoznik (naše vozilo).");

        if (!dto.IzlaznaCena.HasValue)
            throw new ValidationException("IzlaznaCena", "Izlazna cena je obavezna za interni nalog.");

        if (string.IsNullOrWhiteSpace(dto.Valuta))
            throw new ValidationException("Valuta", "Valuta je obavezna za interni nalog.");

        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1) Kreiraj Turu (vozilo iz Kruga)
            var turaBroj = await _turaRepository.GetNextTuraBrojAsync();
            var tura = new Tura
            {
                RedniBroj = turaBroj,
                MestoUtovara = dto.MestoUtovara,
                MestoIstovara = dto.MestoIstovara,
                DatumUtovara = dto.DatumUtovara,
                DatumIstovara = dto.DatumIstovara,
                KolicinaRobe = dto.KolicinaRobe,
                Tezina = dto.Tezina,
                VrstaNadogradnjeId = dto.VrstaNadogradnjeId,
                KlijentId = dto.KlijentId,
                PrevoznikId = dto.PrevoznikId,
                VoziloId = krug.VoziloId,
                KrugId = krug.KrugId,
                IzlaznaCena = dto.IzlaznaCena,
                UlaznaCena = dto.UlaznaCena,
                Valuta = dto.Valuta,
                IzvoznoCarinjenje = dto.IzvoznoCarinjenje,
                UvoznoCarinjenje = dto.UvoznoCarinjenje,
                Napomena = dto.Napomena,
                NapomenaKlijenta = dto.NapomenaKlijenta,
                StatusTure = "Kreiran Nalog"
            };

            _turaRepository.Add(tura);
            await _turaRepository.SaveChangesAsync();

            // 2) Učitaj turu sa navigationima (potrebno za EnsureInternalForTuraAsync)
            var turaFull = await _turaRepository.GetByIdAsync(tura.TuraId)
                ?? throw new NotFoundException("Tura", "Tura nije pronađena nakon kreiranja.");

            // 3) Ensure-uj interni Nalog kroz postojeću logiku NalogService-a
            var (nalog, _) = await _nalogService.EnsureInternalForTuraAsync(turaFull);
            await _nalogPrihodiService.EnsureSeededInitialPrihodAsync(nalog, turaFull);
            await _context.SaveChangesAsync();

            await tx.CommitAsync();

            var created = await _nalogRepository.GetByIdAsync(nalog.NalogId);
            return created!.Adapt<NalogReadDto>();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task CloseAsync(int krugId)
    {
        var krug = await _repository.GetByIdAsync(krugId)
            ?? throw new NotFoundException("Krug", $"Krug sa ID {krugId} nije pronađen.");

        if (krug.Status == "Zatvoren")
            return;

        var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

        krug.Status = "Zatvoren";
        krug.EndAt = DateTime.UtcNow;
        krug.ClosedAt = DateTime.UtcNow;
        krug.ClosedBy = username;

        _repository.Update(krug);
        await _repository.SaveChangesAsync();
    }

    public async Task DeleteAsync(int krugId)
    {
        var krug = await _repository.GetByIdWithTureAsync(krugId)
            ?? throw new NotFoundException("Krug", $"Krug sa ID {krugId} nije pronađen.");

        if (krug.Status != "Otvoren")
            throw new ValidationException("Krug", "Samo otvoren krug može biti obrisan.");

        if (krug.Ture.Any())
            throw new ConflictException("Krug", "Krug ima vezane ture i ne može biti obrisan. Prvo izbacite ture iz kruga.");

        _repository.Delete(krug);
        await _repository.SaveChangesAsync();
    }

    private static KrugReadDto MapReadDto(Krug krug)
    {
        return new KrugReadDto
        {
            KrugId = krug.KrugId,
            Broj = krug.Broj,
            VoziloId = krug.VoziloId,
            VoziloNaziv = krug.Vozilo?.Naziv,
            StartAt = krug.StartAt,
            EndAt = krug.EndAt,
            Status = krug.Status,
            Napomena = krug.Napomena,
            CreatedAt = krug.CreatedAt,
            CreatedBy = krug.CreatedBy,
            ClosedAt = krug.ClosedAt,
            ClosedBy = krug.ClosedBy,
            BrojTura = krug.Ture?.Count ?? 0,
            BrojNaloga = 0
        };
    }

    private static KrugTrosakDto MapKrugTrosakDto(KrugTrosak t)
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

    private static (
        List<AmountByCurrencyDto> TroskoviKruga,
        List<AmountByCurrencyDto> TroskoviNaloga,
        List<AmountByCurrencyDto> Prihodi,
        List<AmountByCurrencyDto> Profit)
        BuildFinancialSummary(Krug krug, IEnumerable<Nalog> nalozi)
    {
        var nalozList = nalozi as IList<Nalog> ?? nalozi.ToList();

        var troskoviKruga = BuildTotals(krug.Troskovi.Select(t => (t.Valuta, t.Iznos)));
        var troskoviNaloga = BuildTotals(nalozList.SelectMany(n => n.Troskovi).Select(t => (t.Valuta, t.Iznos)));
        var prihodi = BuildTotals(nalozList.SelectMany(n => n.Prihodi).Select(p => (p.Valuta, p.Iznos)));

        var allTroskovi = krug.Troskovi.Select(t => (t.Valuta, t.Iznos))
            .Concat(nalozList.SelectMany(n => n.Troskovi).Select(t => (t.Valuta, t.Iznos)));
        var totalTroskovi = BuildTotals(allTroskovi);

        var sveValute = prihodi.Select(x => x.Currency)
            .Concat(totalTroskovi.Select(x => x.Currency))
            .Distinct()
            .OrderBy(v => v);

        var profit = sveValute.Select(v => new AmountByCurrencyDto
        {
            Currency = v,
            Amount = (prihodi.FirstOrDefault(x => x.Currency == v)?.Amount ?? 0m)
                   - (totalTroskovi.FirstOrDefault(x => x.Currency == v)?.Amount ?? 0m)
        }).ToList();

        return (troskoviKruga, troskoviNaloga, prihodi, profit);
    }

    private static List<AmountByCurrencyDto> BuildTotals(IEnumerable<(string Currency, decimal Amount)> values)
    {
        return values
            .GroupBy(x => NormalizeCurrency(x.Currency))
            .Select(g => new AmountByCurrencyDto
            {
                Currency = g.Key,
                Amount = g.Sum(x => x.Amount)
            })
            .OrderBy(x => x.Currency)
            .ToList();
    }

    private static string NormalizeCurrency(string? currency)
    {
        return string.IsNullOrWhiteSpace(currency) ? "RSD" : currency.Trim().ToUpperInvariant();
    }

    private static string FormatKrugBroj(string rawCounterNumber)
    {
        var normalized = string.IsNullOrWhiteSpace(rawCounterNumber)
            ? "000/00"
            : rawCounterNumber.Trim().ToUpperInvariant();

        return normalized.StartsWith("K-") ? normalized : $"K-{normalized}";
    }
}
