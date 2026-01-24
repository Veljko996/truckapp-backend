using Microsoft.EntityFrameworkCore;
using WebApplication1.DataAccess;
using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.DashboardRepository;

public class DashboardRepository : IDashboardRepository
{
    private readonly TruckContext _context;
    private readonly ILogger<DashboardRepository> _logger;

    public DashboardRepository(TruckContext context, ILogger<DashboardRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<int> GetTotalTureCountAsync()
    {
        return await _context.Ture.CountAsync();
    }

    public async Task<int> GetAktivneTureCountAsync()
    {
        return await _context.Ture
            .CountAsync(t => t.StatusTure != "Kreiran Nalog");
    }

    public async Task<Dictionary<string, int>> GetTureStatusDistribucijaAsync()
    {
        var result = await _context.Ture
            .GroupBy(t => t.StatusTure)
            .Select(g => new { Status = g.Key ?? "Nepoznat", Count = g.Count() })
            .ToListAsync();
        
        return result.ToDictionary(x => x.Status, x => x.Count);
    }

    public async Task<List<Tura>> GetTopTureByPriceAsync(int take, CancellationToken cancellationToken = default)
    {
        return await _context.Ture
            .Include(t => t.Prevoznik)
            .Include(t => t.Vozilo)
            .Include(t => t.Klijent)
            .Where(t => t.UlaznaCena.HasValue && t.UlaznaCena > 0)
            .OrderByDescending(t => t.UlaznaCena)
            .Take(take)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetPrihodZaDanasAsync()
    {
        var danas = DateTime.UtcNow.Date;
        return await _context.Ture
            .Where(t => t.DatumUtovara.HasValue && t.DatumUtovara.Value.Date == danas && t.UlaznaCena.HasValue)
            .SumAsync(t => t.UlaznaCena ?? 0);
    }

    public async Task<decimal> GetPrihodZaPeriodAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Ture
            .Where(t => t.DatumUtovara.HasValue && t.DatumUtovara.Value.Date >= startDate.Date && t.DatumUtovara.Value.Date <= endDate.Date && t.UlaznaCena.HasValue)
            .SumAsync(t => t.UlaznaCena ?? 0);
    }

    public async Task<List<(DateTime Datum, decimal Suma)>> GetPrihodByDateAsync(DateTime startDate, DateTime endDate)
    {
        var prihod = await _context.Ture
            .Where(t => t.DatumUtovara.HasValue
                && t.DatumUtovara.Value.Date >= startDate.Date
                && t.DatumUtovara.Value.Date <= endDate.Date
                && t.UlaznaCena.HasValue)
            .GroupBy(t => t.DatumUtovara!.Value.Date)
            .Select(g => new { Datum = g.Key, Suma = g.Sum(t => t.UlaznaCena ?? 0) })
            .OrderBy(x => x.Datum)
            .AsNoTracking()
            .ToListAsync();

        return prihod.Select(p => (p.Datum, p.Suma)).ToList();
    }

    public async Task<int> GetTotalNaloziCountAsync()
    {
        return await _context.Nalozi.CountAsync();
    }

    public async Task<int> GetAktivniNaloziCountAsync()
    {
        var neaktivniStatusi = new[] { "Završen", "Ponisten", "Storniran" };
        return await _context.Nalozi.CountAsync(n => n.StatusNaloga != null && !neaktivniStatusi.Contains(n.StatusNaloga));
    }

    public async Task<Dictionary<string, int>> GetNaloziStatusDistribucijaAsync()
    {
        var result = await _context.Nalozi
            .Where(n => n.StatusNaloga != null)
            .GroupBy(n => n.StatusNaloga!)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();
        
        return result.ToDictionary(x => x.Status, x => x.Count);
    }

    public async Task<int> GetTotalVozilaCountAsync()
    {
        return await _context.NasaVozila.CountAsync();
    }

    public async Task<int> GetAktivnaVozilaCountAsync()
    {
        var aktivniStatusi = new[] { "Slobodno", "Na Putu", "Zauzeto" };
        return await _context.NasaVozila.CountAsync(v => v.Raspolozivost != null && aktivniStatusi.Contains(v.Raspolozivost));
    }

    public async Task<List<NasaVozila>> GetVozilaSaIsticucimDokumentimaAsync(int daysThreshold, CancellationToken cancellationToken = default)
    {
        var danas = DateTime.UtcNow.Date;
        var thresholdDate = danas.AddDays(daysThreshold);

        return await _context.NasaVozila
            .Include(v => v.Vinjete)
            .Where(v =>
                // Registracija
                (v.RegistracijaDatumIsteka.HasValue
                    && v.RegistracijaDatumIsteka.Value.Date >= danas
                    && v.RegistracijaDatumIsteka.Value.Date <= thresholdDate) ||
                // Tehnički pregled
                (v.TehnickiPregledDatumIsteka.HasValue
                    && v.TehnickiPregledDatumIsteka.Value.Date >= danas
                    && v.TehnickiPregledDatumIsteka.Value.Date <= thresholdDate) ||
                // PP Aparat
                (v.PPAparatDatumIsteka.HasValue
                    && v.PPAparatDatumIsteka.Value.Date >= danas
                    && v.PPAparatDatumIsteka.Value.Date <= thresholdDate) ||
                // Vinjete
                v.Vinjete.Any(vin =>
                    vin.DatumIsteka.Date >= danas
                    && vin.DatumIsteka.Date <= thresholdDate))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<NasaVozila>> GetVozilaZaDashboardAsync(CancellationToken cancellationToken = default)
    {
        return await _context.NasaVozila
            .Include(v => v.Vinjete)
            .Include(v => v.Ture)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetIsticeVinjetaCountAsync(int daysThreshold)
    {
        var danas = DateTime.UtcNow.Date;
        var thresholdDate = danas.AddDays(daysThreshold);
        return await _context.Vinjete.CountAsync(v => v.DatumIsteka.Date >= danas && v.DatumIsteka.Date <= thresholdDate);
    }

    public async Task<List<Log>> GetNajnovijiLogoviAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _context.Logs
            .Include(l => l.User)
            .OrderByDescending(l => l.HappenedAtDate)
            .Take(count)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetAktivniKlijentiCountAsync()
    {
        return await _context.Klijenti.CountAsync(k => k.Aktivan);
    }

    public async Task<int> GetAktivniPrevozniciCountAsync()
    {
        return await _context.Prevoznici.CountAsync();
    }

    public async Task<int> GetActiveNalogsCountAsync()
    {
        return await _context.Nalozi
            .CountAsync(n => n.StatusNaloga == "U Toku");
    }

    public async Task<int> GetLateUnloadNalogsCountAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _context.Nalozi
            .CountAsync(n => n.StatusNaloga != "Završen"
                && (n.Istovar == false || n.Istovar == null)
                && n.DatumIstovara.HasValue
                && n.DatumIstovara.Value.Date < today);
    }

    public async Task<(decimal ProfitEUR, decimal ProfitRSD)> GetProfitLast30DaysAsync()
    {
        var startDate = DateTime.UtcNow.AddDays(-30).Date;
        var endDate = DateTime.UtcNow.Date;

        var ture = await _context.Ture
            .Where(t => t.DatumUtovara.HasValue
                && t.DatumUtovara.Value.Date >= startDate
                && t.DatumUtovara.Value.Date <= endDate
                && t.UlaznaCena.HasValue
                && t.IzlaznaCena.HasValue
                && _context.Nalozi.Any(n => n.TuraId == t.TuraId && n.StatusNaloga == "Završen"))
            .Select(t => new
            {
                Valuta = t.Valuta ?? "RSD",
                Profit = (t.IzlaznaCena ?? 0) - (t.UlaznaCena ?? 0)
            })
            .AsNoTracking()
            .ToListAsync();

        var profitEUR = ture
            .Where(t => t.Valuta == "EUR")
            .Sum(t => t.Profit);

        var profitRSD = ture
            .Where(t => t.Valuta == "RSD" || t.Valuta == null)
            .Sum(t => t.Profit);

        return (profitEUR, profitRSD);
    }

    public async Task<List<Tura>> GetTopTureByProfitAsync(int take, CancellationToken cancellationToken = default)
    {
        return await _context.Ture
            .Include(t => t.Prevoznik)
            .Include(t => t.Vozilo)
            .Include(t => t.Klijent)
            .Where(t => t.UlaznaCena.HasValue 
                && t.IzlaznaCena.HasValue
                && t.DatumUtovara.HasValue
                && t.DatumUtovara.Value.Date >= DateTime.UtcNow.AddDays(-30).Date
                && _context.Nalozi.Any(n => n.TuraId == t.TuraId && n.StatusNaloga == "Završen"))
            .OrderByDescending(t => Math.Abs((t.IzlaznaCena ?? 0) - (t.UlaznaCena ?? 0)))
            .Take(take)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<(int PrevoznikId, string Naziv, int TotalToursCount)>> GetTop5CarriersLast30DaysAsync(CancellationToken cancellationToken = default)
    {
        var startDate = DateTime.UtcNow.AddDays(-30).Date;
        var endDate = DateTime.UtcNow.Date;

        var result = await _context.Ture
            .Include(t => t.Prevoznik)
            .Where(t => t.PrevoznikId.HasValue
                && t.DatumUtovara.HasValue
                && t.DatumUtovara.Value.Date >= startDate
                && t.DatumUtovara.Value.Date <= endDate)
            .GroupBy(t => new { t.PrevoznikId, Naziv = t.Prevoznik != null ? t.Prevoznik.Naziv : "Unknown" })
            .Select(g => new
            {
                PrevoznikId = g.Key.PrevoznikId!.Value,
                Naziv = g.Key.Naziv,
                TotalToursCount = g.Count()
            })
            .OrderByDescending(x => x.TotalToursCount)
            .Take(5)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return result.Select(r => (r.PrevoznikId, r.Naziv, r.TotalToursCount)).ToList();
    }

    public async Task<List<(int KlijentId, string NazivFirme, decimal ProfitEUR, decimal ProfitRSD)>> GetTop5ClientsByProfitLast30DaysAsync(CancellationToken cancellationToken = default)
    {
        var startDate = DateTime.UtcNow.AddDays(-30).Date;
        var endDate = DateTime.UtcNow.Date;

        var ture = await _context.Ture
            .Include(t => t.Klijent)
            .Where(t => t.KlijentId.HasValue
                && t.DatumUtovara.HasValue
                && t.DatumUtovara.Value.Date >= startDate
                && t.DatumUtovara.Value.Date <= endDate
                && t.UlaznaCena.HasValue
                && t.IzlaznaCena.HasValue
                && _context.Nalozi.Any(n => n.TuraId == t.TuraId && n.StatusNaloga == "Završen"))
            .Select(t => new
            {
                KlijentId = t.KlijentId!.Value,
                NazivFirme = t.Klijent != null ? t.Klijent.NazivFirme : "Unknown",
                Valuta = t.Valuta ?? "RSD",
                Profit = (t.IzlaznaCena ?? 0) - (t.UlaznaCena ?? 0)
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var grouped = ture
            .GroupBy(t => new { t.KlijentId, t.NazivFirme })
            .Select(g => new
            {
                g.Key.KlijentId,
                g.Key.NazivFirme,
                ProfitEUR = g.Where(t => t.Valuta == "EUR").Sum(t => t.Profit),
                ProfitRSD = g.Where(t => t.Valuta == "RSD" || t.Valuta == null).Sum(t => t.Profit)
            })
            .Select(x => new
            {
                x.KlijentId,
                x.NazivFirme,
                TotalProfit = Math.Abs(x.ProfitEUR) + Math.Abs(x.ProfitRSD),
                ProfitEUR = x.ProfitEUR,
                ProfitRSD = x.ProfitRSD
            })
            .OrderByDescending(x => x.TotalProfit)
            .Take(5)
            .ToList();

        return grouped.Select(x => (x.KlijentId, x.NazivFirme, x.ProfitEUR, x.ProfitRSD)).ToList();
    }

    public async Task<List<Nalog>> GetLateUnloadNalogsAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.Nalozi
            .Include(n => n.Tura!)
                .ThenInclude(t => t.Klijent)
            .Where(n => n.StatusNaloga != "Završen"
                && (n.Istovar == false || n.Istovar == null)
                && n.DatumIstovara.HasValue
                && n.DatumIstovara.Value.Date < today)
            .OrderBy(n => n.DatumIstovara)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
