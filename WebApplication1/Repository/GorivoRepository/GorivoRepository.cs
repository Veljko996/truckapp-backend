using Microsoft.EntityFrameworkCore;
using WebApplication1.DataAccess;
using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.GorivoRepository;

public class GorivoRepository : IGorivoRepository
{
    private readonly TruckContext _context;

    public GorivoRepository(TruckContext context)
    {
        _context = context;
    }

    public async Task<List<GorivoZapis>> GetByVoziloIdAsync(int voziloId)
    {
        return await _context.GorivoZapisi
            .Include(g => g.Vozilo)
            .Include(g => g.Nalog)
            .Where(g => g.VoziloId == voziloId)
            .AsNoTracking()
            .OrderByDescending(g => g.DatumTocenja)
            .ThenByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<GorivoZapis>> GetByNalogIdAsync(int nalogId)
    {
        return await _context.GorivoZapisi
            .Include(g => g.Vozilo)
            .Include(g => g.Nalog)
            .Where(g => g.NalogId == nalogId)
            .AsNoTracking()
            .OrderByDescending(g => g.DatumTocenja)
            .ThenByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    public async Task<GorivoZapis?> GetByIdAsync(int gorivoZapisId)
    {
        return await _context.GorivoZapisi
            .FirstOrDefaultAsync(g => g.GorivoZapisId == gorivoZapisId);
    }

    public void Add(GorivoZapis entity)
    {
        _context.GorivoZapisi.Add(entity);
    }

    public void Delete(GorivoZapis entity)
    {
        _context.GorivoZapisi.Remove(entity);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}
