using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Services.NalogVozacAccessServices;

public class NalogVozacAccessService : INalogVozacAccessService
{
    private readonly TruckContext _context;

    public NalogVozacAccessService(TruckContext context)
    {
        _context = context;
    }

    public IQueryable<Nalog> ApplyVozacFilter(IQueryable<Nalog> query, int userId)
    {
        return query.Where(n =>
            n.Tura != null &&
            n.Tura.VoziloId != null &&
            _context.NasaVoziloVozacAssignments.Any(a =>
                a.VoziloId == n.Tura.VoziloId.Value &&
                a.UnassignedAt == null &&
                a.Employee!.UserId == userId
            )
        );
    }

    public Task<bool> CanAccessNalogAsync(int userId, int nalogId, CancellationToken ct = default)
    {
        return ApplyVozacFilter(_context.Nalozi, userId)
            .AnyAsync(n => n.NalogId == nalogId, ct);
    }

    public Task<bool> CanAccessVoziloAsync(int userId, int voziloId, CancellationToken ct = default)
    {
        return _context.NasaVoziloVozacAssignments
            .AnyAsync(a =>
                a.VoziloId == voziloId &&
                a.UnassignedAt == null &&
                a.Employee!.UserId == userId, ct);
    }
}
