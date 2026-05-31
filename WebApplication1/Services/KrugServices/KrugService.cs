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
        var vozaciByVoziloId = await GetActiveVozaciByVoziloIdsAsync(krugovi.Select(k => k.VoziloId));

        return krugovi
            .Select(k => MapReadDto(k, vozaciByVoziloId.GetValueOrDefault(k.VoziloId)))
            .ToList();
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
        var vozaciByVoziloId = await GetActiveVozaciByVoziloIdsAsync(new[] { krug.VoziloId });
        var activeVozac = vozaciByVoziloId.GetValueOrDefault(krug.VoziloId);

        var details = new KrugDetailsDto
        {
            KrugId = krug.KrugId,
            Broj = krug.Broj,
            VoziloId = krug.VoziloId,
            VoziloNaziv = krug.Vozilo?.Naziv,
            VozacId = activeVozac?.VozacId,
            VozacImePrezime = activeVozac?.ImePrezime ?? "Nema",
            PrimarniNalogIdZaDokumente = nalozi
                .OrderByDescending(n => n.NalogId)
                .FirstOrDefault()
                ?.NalogId,
            StartAt = krug.StartAt,
            EndAt = krug.EndAt,
            PocetnaKilometraza = krug.PocetnaKilometraza,
            ZavrsnaKilometraza = krug.ZavrsnaKilometraza,
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
        var gorivoZapisi = await GetGorivoByKrugAsync(krug, nalozi.Select(n => n.NalogId));
        var (troskoviKruga, gorivo, troskoviNaloga, prihodi, profit) = BuildFinancialSummary(krug, nalozi, gorivoZapisi);
        details.UkupniTroskoviKrugaPoValuti = troskoviKruga;
        details.UkupnoGorivoPoValuti = gorivo;
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

        var gorivoZapisi = await GetGorivoByKrugAsync(krug, nalozi.Select(n => n.NalogId));
        var (troskoviKruga, gorivo, troskoviNaloga, prihodi, profit) = BuildFinancialSummary(krug, nalozi, gorivoZapisi);

        return new KrugFinancialSummaryDto
        {
            KrugId = krug.KrugId,
            Broj = krug.Broj,
            Status = krug.Status,
            BrojTura = krug.Ture.Count,
            BrojNaloga = nalozi.Count,
            UkupniTroskoviKrugaPoValuti = troskoviKruga,
            UkupnoGorivoPoValuti = gorivo,
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
        var source = full ?? open;
        var vozaciByVoziloId = await GetActiveVozaciByVoziloIdsAsync(new[] { source.VoziloId });
        return MapReadDto(source, vozaciByVoziloId.GetValueOrDefault(source.VoziloId));
    }

    public async Task<KrugReadDto> CreateAsync(CreateKrugDto dto)
    {
        var vozilo = await _context.NasaVozila.FindAsync(dto.VoziloId)
            ?? throw new ValidationException("Vozilo", $"Vozilo sa ID {dto.VoziloId} ne postoji.");

        ValidateKilometraza("PocetnaKilometraza", dto.PocetnaKilometraza);

        var existingOpen = await _repository.GetOpenByVoziloIdAsync(dto.VoziloId);
        if (existingOpen != null)
            throw new ConflictException("Krug", $"Vozilo '{vozilo.Naziv}' već ima otvoren krug (#{existingOpen.KrugId}).");

        var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

        await using var tx = await _context.Database.BeginTransactionAsync();

        var nextBroj = await _repository.GetNextKrugBrojAsync();

        var krug = new Krug
        {
            Broj = FormatKrugBroj(nextBroj),
            VoziloId = dto.VoziloId,
            StartAt = dto.StartAt ?? DateTime.UtcNow,
            PocetnaKilometraza = dto.PocetnaKilometraza ?? vozilo.Kilometraza,
            Status = "Otvoren",
            Napomena = dto.Napomena,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = username
        };

        _repository.Add(krug);
        await _repository.SaveChangesAsync();
        await tx.CommitAsync();

        var created = await _repository.GetByIdAsync(krug.KrugId);
        var vozaciByVoziloId = await GetActiveVozaciByVoziloIdsAsync(new[] { created!.VoziloId });
        return MapReadDto(created, vozaciByVoziloId.GetValueOrDefault(created.VoziloId));
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

        var vozilo = await _context.NasaVozila.FindAsync(voziloId.Value)
            ?? throw new ValidationException("Vozilo", $"Vozilo sa ID {voziloId.Value} ne postoji.");

        if (nalog.Tura.KrugId.HasValue)
            throw new ConflictException("Krug", $"Tura ovog naloga je već u krugu (#{nalog.Tura.KrugId.Value}).");

        var existingOpen = await _repository.GetOpenByVoziloIdAsync(voziloId.Value);
        if (existingOpen != null)
            throw new ConflictException("Krug", $"Vozilo već ima otvoren krug (#{existingOpen.KrugId}).");

        var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

        await using var tx = await _context.Database.BeginTransactionAsync();

        var nextBroj = await _repository.GetNextKrugBrojAsync();

        var krug = new Krug
        {
            Broj = FormatKrugBroj(nextBroj),
            VoziloId = voziloId.Value,
            StartAt = DateTime.UtcNow,
            PocetnaKilometraza = vozilo.Kilometraza,
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
        var vozaciByVoziloId = await GetActiveVozaciByVoziloIdsAsync(new[] { created!.VoziloId });
        return MapReadDto(created, vozaciByVoziloId.GetValueOrDefault(created.VoziloId));
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

    public async Task CloseAsync(int krugId, CloseKrugDto? dto = null)
    {
        var krug = await _repository.GetByIdAsync(krugId)
            ?? throw new NotFoundException("Krug", $"Krug sa ID {krugId} nije pronađen.");

        if (krug.Status == "Zatvoren")
            return;

        var zavrsnaKilometraza = dto?.ZavrsnaKilometraza;
        ValidateKilometraza("ZavrsnaKilometraza", zavrsnaKilometraza);

        if (krug.PocetnaKilometraza.HasValue
            && zavrsnaKilometraza.HasValue
            && zavrsnaKilometraza.Value < krug.PocetnaKilometraza.Value)
        {
            throw new ValidationException("ZavrsnaKilometraza", "Završna kilometraža ne može biti manja od početne.");
        }

        var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

        krug.Status = "Zatvoren";
        krug.EndAt = DateTime.UtcNow;
        krug.ClosedAt = DateTime.UtcNow;
        krug.ClosedBy = username;

        if (zavrsnaKilometraza.HasValue)
        {
            krug.ZavrsnaKilometraza = zavrsnaKilometraza.Value;
            var vozilo = krug.Vozilo ?? await _context.NasaVozila.FindAsync(krug.VoziloId);
            if (vozilo != null)
                vozilo.Kilometraza = zavrsnaKilometraza.Value;
        }

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

    private async Task<Dictionary<int, ActiveVozacInfo>> GetActiveVozaciByVoziloIdsAsync(IEnumerable<int> voziloIds)
    {
        var ids = voziloIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<int, ActiveVozacInfo>();

        var assignments = await _context.NasaVoziloVozacAssignments
            .AsNoTracking()
            .Where(a => ids.Contains(a.VoziloId) && a.UnassignedAt == null)
            .Select(a => new
            {
                a.VoziloId,
                VozacId = a.EmployeeId,
                ImePrezime = a.Employee!.User.FullName,
                a.SlotNumber,
                a.AssignedAt
            })
            .ToListAsync();

        return assignments
            .GroupBy(a => a.VoziloId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var selected = g
                        .OrderBy(a => a.SlotNumber)
                        .ThenByDescending(a => a.AssignedAt)
                        .First();

                    return new ActiveVozacInfo(
                        selected.VozacId,
                        string.IsNullOrWhiteSpace(selected.ImePrezime) ? "Nema" : selected.ImePrezime);
                });
    }

    private static KrugReadDto MapReadDto(Krug krug, ActiveVozacInfo? activeVozac = null)
    {
        return new KrugReadDto
        {
            KrugId = krug.KrugId,
            Broj = krug.Broj,
            VoziloId = krug.VoziloId,
            VoziloNaziv = krug.Vozilo?.Naziv,
            VozacId = activeVozac?.VozacId,
            VozacImePrezime = activeVozac?.ImePrezime ?? "Nema",
            StartAt = krug.StartAt,
            EndAt = krug.EndAt,
            PocetnaKilometraza = krug.PocetnaKilometraza,
            ZavrsnaKilometraza = krug.ZavrsnaKilometraza,
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

    private static void ValidateKilometraza(string fieldName, int? kilometraza)
    {
        if (kilometraza.HasValue && kilometraza.Value < 0)
            throw new ValidationException(fieldName, "Kilometraža ne može biti negativna.");
    }

    private sealed record ActiveVozacInfo(int VozacId, string ImePrezime);

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
        List<AmountByCurrencyDto> Gorivo,
        List<AmountByCurrencyDto> TroskoviNaloga,
        List<AmountByCurrencyDto> Prihodi,
        List<AmountByCurrencyDto> Profit)
        BuildFinancialSummary(Krug krug, IEnumerable<Nalog> nalozi, IEnumerable<GorivoZapis> gorivoZapisi)
    {
        var nalozList = nalozi as IList<Nalog> ?? nalozi.ToList();
        var gorivoList = gorivoZapisi as IList<GorivoZapis> ?? gorivoZapisi.ToList();

        var troskoviKruga = BuildTotals(krug.Troskovi.Select(t => (t.Valuta, t.Iznos)));
        var gorivo = BuildTotals(gorivoList.Select(g => (g.Valuta, g.Iznos)));
        var troskoviNaloga = BuildTotals(nalozList.SelectMany(n => n.Troskovi).Select(t => (t.Valuta, t.Iznos)));
        var prihodi = BuildTotals(nalozList.SelectMany(n => n.Prihodi).Select(p => (p.Valuta, p.Iznos)));

        var allTroskovi = krug.Troskovi.Select(t => (t.Valuta, t.Iznos))
            .Concat(gorivoList.Select(g => (g.Valuta, g.Iznos)))
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

        return (troskoviKruga, gorivo, troskoviNaloga, prihodi, profit);
    }

    /// <summary>
    /// Determinističko pravilo za gorivo u rezimeu kruga:
    ///   1) GorivoZapis.KrugId == krug.KrugId
    ///   2) ili GorivoZapis.NalogId pripada nalozima tog kruga.
    /// Bez datum/vozilo heuristike.
    /// </summary>
    private async Task<List<GorivoZapis>> GetGorivoByKrugAsync(Krug krug, IEnumerable<int> nalogIds)
    {
        var nalogIdList = nalogIds.ToList();

        return await _context.GorivoZapisi
            .AsNoTracking()
            .Where(g =>
                g.KrugId == krug.KrugId
                || (g.NalogId.HasValue && nalogIdList.Contains(g.NalogId.Value)))
            .ToListAsync();
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
