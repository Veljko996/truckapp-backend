using Microsoft.EntityFrameworkCore;
using WebApplication1.DataAccess;

namespace WebApplication1.Repository.ReportRepository;

public class ReportRepository : IReportRepository
{
    private readonly TruckContext _context;

    public ReportRepository(TruckContext context)
    {
        _context = context;
    }

    public async Task<KrugReportData> GetKrugReportDataAsync(DateTime from, DateTime to, int? voziloId)
    {
        // 1) Krugovi zatvoreni u opsegu (po datumu zatvaranja). EF global filter dodaje TenantId.
        var krugQuery = _context.Krugovi
            .AsNoTracking()
            .Include(k => k.Vozilo)
            .Include(k => k.Troskovi)
            .Include(k => k.Ture)
            .Where(k => k.Status == "Zatvoren"
                        && k.ClosedAt != null
                        && k.ClosedAt >= from
                        && k.ClosedAt <= to);

        if (voziloId.HasValue)
            krugQuery = krugQuery.Where(k => k.VoziloId == voziloId.Value);

        var krugovi = await krugQuery.ToListAsync();

        var tureIds = krugovi.SelectMany(k => k.Ture.Select(t => t.TuraId)).Distinct().ToList();
        var krugIds = krugovi.Select(k => k.KrugId).ToList();

        // 2) Svi nalozi tih tura (osim Storniran/Ponisten) — isti kriterijum kao krug rezime.
        var nalozi = tureIds.Count == 0
            ? new List<DataAccess.Models.Nalog>()
            : await _context.Nalozi
                .AsNoTracking()
                .Include(n => n.Troskovi)
                .Include(n => n.Prihodi)
                .Where(n => tureIds.Contains(n.TuraId)
                            && n.StatusNaloga != "Storniran"
                            && n.StatusNaloga != "Ponisten")
                .ToListAsync();

        var nalogIds = nalozi.Select(n => n.NalogId).ToList();

        // 3) Gorivo vezano za krug (KrugId) ILI za neki od naloga (NalogId) — isto pravilo kao GetGorivoByKrugAsync.
        var gorivo = (krugIds.Count == 0 && nalogIds.Count == 0)
            ? new List<DataAccess.Models.GorivoZapis>()
            : await _context.GorivoZapisi
                .AsNoTracking()
                .Where(g => (g.KrugId.HasValue && krugIds.Contains(g.KrugId.Value))
                            || (g.NalogId.HasValue && nalogIds.Contains(g.NalogId.Value)))
                .ToListAsync();

        return new KrugReportData { Krugovi = krugovi, Nalozi = nalozi, Gorivo = gorivo };
    }
}
