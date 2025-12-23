using Microsoft.EntityFrameworkCore;
using WebApplication1.DataAccess;
using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.DashboardRepository;

/// <summary>
/// Repository implementation for dashboard data aggregation.
/// Optimized queries with proper indexes and async execution.
/// </summary>
public class DashboardRepository : IDashboardRepository
{
    private readonly TruckContext _context;
    private readonly ILogger<DashboardRepository> _logger;

    public DashboardRepository(TruckContext context, ILogger<DashboardRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    // === Ture Statistics ===

    public async Task<int> GetTotalTureCountAsync()
    {
        return await _context.Ture.CountAsync();
    }

    public async Task<int> GetAktivneTureCountAsync()
    {
        var aktivniStatusi = new[] { "U Toku", "Kreirana", "Na Putu", "Utovar U Toku", "Istovar U Toku", "Carina" };
        return await _context.Ture
            .CountAsync(t => aktivniStatusi.Contains(t.StatusTure));
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
            .AsNoTracking() // Read-only optimization
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetPrihodZaDanasAsync()
    {
        try
        {
            var danas = DateTime.UtcNow.Date;
            var result = await _context.Ture
                .Where(t => t.DatumUtovara.HasValue 
                    && t.DatumUtovara.Value.Date == danas 
                    && t.UlaznaCena.HasValue)
                .SumAsync(t => t.UlaznaCena ?? 0);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetPrihodZaDanasAsync");
            return 0;
        }
    }

    public async Task<decimal> GetPrihodZaPeriodAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var result = await _context.Ture
                .Where(t => t.DatumUtovara.HasValue
                    && t.DatumUtovara.Value.Date >= startDate.Date
                    && t.DatumUtovara.Value.Date <= endDate.Date
                    && t.UlaznaCena.HasValue)
                .SumAsync(t => t.UlaznaCena ?? 0);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetPrihodZaPeriodAsync");
            return 0;
        }
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

    // === Nalozi Statistics ===

    public async Task<int> GetTotalNaloziCountAsync()
    {
        return await _context.Nalozi.CountAsync();
    }

    public async Task<int> GetAktivniNaloziCountAsync()
    {
        var aktivniStatusi = new[] { "U Toku", "Kreiran" };
        return await _context.Nalozi
            .CountAsync(n => n.StatusNaloga != null && aktivniStatusi.Contains(n.StatusNaloga));
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

    // === Vozila Statistics ===

    public async Task<int> GetTotalVozilaCountAsync()
    {
        return await _context.NasaVozila.CountAsync();
    }

    public async Task<int> GetAktivnaVozilaCountAsync()
    {
        var aktivniStatusi = new[] { "Slobodno", "Na Putu", "Zauzeto" };
        return await _context.NasaVozila
            .CountAsync(v => v.Raspolozivost != null && aktivniStatusi.Contains(v.Raspolozivost));
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

    // === Vinjete Statistics ===

    public async Task<int> GetIsticeVinjetaCountAsync(int daysThreshold)
    {
        var danas = DateTime.UtcNow.Date;
        var thresholdDate = danas.AddDays(daysThreshold);

        return await _context.Vinjete
            .CountAsync(v => v.DatumIsteka.Date >= danas && v.DatumIsteka.Date <= thresholdDate);
    }

    // === Other ===

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
}
