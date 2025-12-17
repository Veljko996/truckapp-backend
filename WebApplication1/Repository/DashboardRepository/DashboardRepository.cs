using Microsoft.EntityFrameworkCore;
using WebApplication1.DataAccess;
using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.DashboardRepository;

public class DashboardRepository : IDashboardRepository
{
    private readonly TruckContext _context;

    public DashboardRepository(TruckContext context)
    {
        _context = context;
    }

    public async Task<int> GetAllTureCountAsync()
    {
        return await _context.Ture.CountAsync();
    }

    public async Task<List<(string Status, int Count)>> GetStatusDistribucijaAsync()
    {
        var distribucija = await _context.Ture
            .GroupBy(t => t.StatusTure)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        return distribucija.Select(d => (d.Status, d.Count)).ToList();
    }

    public async Task<List<(DateTime Datum, decimal Suma)>> GetPrihodByDateAsync()
    {
        var prihod = await _context.Ture
            .Where(t => t.DatumUtovaraOd.HasValue && t.UlaznaCena.HasValue)
            .GroupBy(t => t.DatumUtovaraOd!.Value.Date)
            .Select(g => new { Datum = g.Key, Suma = g.Sum(t => t.UlaznaCena ?? 0) })
            .OrderBy(x => x.Datum)
            .ToListAsync();

        return prihod.Select(p => (p.Datum, p.Suma)).ToList();
    }

    public async Task<List<Tura>> GetAllTureWithIncludesAsync()
    {
        return await _context.Ture
            .Include(t => t.Prevoznik)
            .Include(t => t.Vozilo)
            .Include(t => t.Klijent)
            .Include(t => t.VrstaNadogradnje)
            .ToListAsync();
    }

    public async Task<List<NasaVozila>> GetAllVozilaWithIncludesAsync()
    {
        return await _context.NasaVozila
            .Include(v => v.Vinjete)
            .Include(v => v.Ture)
            .ToListAsync();
    }

    public async Task<List<Log>> GetNajnovijiLogoviAsync(int count = 10)
    {
        return await _context.Logs
            .Include(l => l.User)
            .OrderByDescending(l => l.HappenedAtDate)
            .Take(count)
            .ToListAsync();
    }
}

