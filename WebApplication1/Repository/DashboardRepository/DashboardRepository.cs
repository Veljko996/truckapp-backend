using Microsoft.EntityFrameworkCore;
using WebApplication1.DataAccess;
using WebApplication1.DataAccess.Models;
using WebApplication1.Utils.Enums;

namespace WebApplication1.Repository.DashboardRepository;

public class DashboardRepository : IDashboardRepository
{
    private readonly TruckContext _context;

    public DashboardRepository(TruckContext context)
    {
        _context = context;
    }

    public async Task<int> GetAktivneTureCountAsync()
    {
        return await _context.Ture
            .CountAsync(t => t.StatusTrenutni == TuraStatus.UPripremi || 
                           t.StatusTrenutni == TuraStatus.NaPutu ||
                           t.StatusTrenutni == TuraStatus.UtovarUToku ||
                           t.StatusTrenutni == TuraStatus.IstovarUToku ||
                           t.StatusTrenutni == TuraStatus.Carina);
    }

    public async Task<decimal> GetDanasnjiPrihodAsync()
    {
        var danas = DateTime.UtcNow.Date;
        var rezultat = await _context.Ture
            .Where(t => t.UtovarDatum.Date == danas && t.UlaznaCena.HasValue)
            .SumAsync(t => t.UlaznaCena ?? 0);
        
        return rezultat;
    }

    public async Task<int> GetAktivnaVozilaCountAsync()
    {
        // Vozila koja su vezana za ture koje nisu Zavrsene
        return await _context.NasaVozila
            .CountAsync(v => v.Ture.Any(t => 
                t.StatusTrenutni != TuraStatus.Zavrseno && 
                t.StatusTrenutni != TuraStatus.Otkazano));
    }

    public async Task<int> GetProblematickeTureCountAsync()
    {
        // StatusKonacni = 'Problem' - proveri da li postoji ovaj status u bazi
        // Ako ne postoji, možda je to neki drugi status
        return await _context.Ture
            .CountAsync(t => t.StatusKonacni == "Problem" || 
                           t.StatusTrenutni == TuraStatus.Zakasnjenje);
    }

    public async Task<int> GetVozilaSaIsticucimDokumentimaCountAsync(int daysThreshold = 7)
    {
        var thresholdDate = DateTime.UtcNow.AddDays(daysThreshold);
        var today = DateTime.UtcNow;

        return await _context.NasaVozila
            .CountAsync(v => 
                (v.RegistracijaDatumIsteka.HasValue && 
                 v.RegistracijaDatumIsteka.Value >= today && 
                 v.RegistracijaDatumIsteka.Value <= thresholdDate) ||
                (v.TehnickiPregledDatumIsteka.HasValue && 
                 v.TehnickiPregledDatumIsteka.Value >= today && 
                 v.TehnickiPregledDatumIsteka.Value <= thresholdDate) ||
                (v.PPAparatDatumIsteka.HasValue && 
                 v.PPAparatDatumIsteka.Value >= today && 
                 v.PPAparatDatumIsteka.Value <= thresholdDate) ||
                v.Vinjete.Any(vin => 
                    vin.DatumIsteka >= today && 
                    vin.DatumIsteka <= thresholdDate));
    }

    public async Task<List<(string Status, int Count)>> GetStatusDistribucijaAsync()
    {
        var distribucija = await _context.Ture
            .GroupBy(t => t.StatusTrenutni)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        return distribucija.Select(d => (d.Status, d.Count)).ToList();
    }

    public async Task<List<(DateTime Datum, decimal Suma)>> GetPrihod30DanaAsync()
    {
        var startDate = DateTime.UtcNow.AddDays(-30).Date;
        
        var prihod = await _context.Ture
            .Where(t => t.UtovarDatum >= startDate && t.UlaznaCena.HasValue)
            .GroupBy(t => t.UtovarDatum.Date)
            .Select(g => new { Datum = g.Key, Suma = g.Sum(t => t.UlaznaCena ?? 0) })
            .OrderBy(x => x.Datum)
            .ToListAsync();

        return prihod.Select(p => (p.Datum, p.Suma)).ToList();
    }

    public async Task<List<Tura>> GetTopTureAsync(int topCount = 5)
    {
        return await _context.Ture
            .Include(t => t.Prevoznik)
            .Include(t => t.Vozilo)
            .Where(t => t.UlaznaCena.HasValue && t.UlaznaCena > 0)
            .OrderByDescending(t => t.UlaznaCena)
            .Take(topCount)
            .ToListAsync();
    }

    public async Task<List<NasaVozila>> GetKriticnaVozilaAsync(int daysThreshold = 7)
    {
        var thresholdDate = DateTime.UtcNow.AddDays(daysThreshold);
        var today = DateTime.UtcNow;

        return await _context.NasaVozila
            .Include(v => v.Vinjete)
            .Where(v => 
                (v.RegistracijaDatumIsteka.HasValue && 
                 v.RegistracijaDatumIsteka.Value >= today && 
                 v.RegistracijaDatumIsteka.Value <= thresholdDate) ||
                (v.TehnickiPregledDatumIsteka.HasValue && 
                 v.TehnickiPregledDatumIsteka.Value >= today && 
                 v.TehnickiPregledDatumIsteka.Value <= thresholdDate) ||
                (v.PPAparatDatumIsteka.HasValue && 
                 v.PPAparatDatumIsteka.Value >= today && 
                 v.PPAparatDatumIsteka.Value <= thresholdDate) ||
                v.Vinjete.Any(vin => 
                    vin.DatumIsteka >= today && 
                    vin.DatumIsteka <= thresholdDate))
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

