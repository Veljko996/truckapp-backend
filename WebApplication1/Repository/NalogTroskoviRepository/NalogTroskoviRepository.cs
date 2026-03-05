using Microsoft.EntityFrameworkCore;
using WebApplication1.DataAccess;
using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.NalogTroskoviRepository;

public class NalogTroskoviRepository : INalogTroskoviRepository
{
    private readonly TruckContext _context;

    public NalogTroskoviRepository(TruckContext context)
    {
        _context = context;
    }

    public async Task<List<NalogTrosak>> GetByNalogIdAsync(int nalogId)
    {
        return await _context.NalogTroskovi
            .Include(t => t.TipTroska)
            .Where(t => t.NalogId == nalogId)
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<NalogTrosak?> GetByIdAsync(int trosakId)
    {
        return await _context.NalogTroskovi
            .FirstOrDefaultAsync(t => t.TrosakId == trosakId);
    }

    public void Add(NalogTrosak entity)
    {
        _context.NalogTroskovi.Add(entity);
    }

    public void Delete(NalogTrosak entity)
    {
        _context.NalogTroskovi.Remove(entity);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<List<TipTroska>> GetAllTipoviAsync()
    {
        return await _context.TipoviTroskova
            .AsNoTracking()
            .OrderBy(t => t.Naziv)
            .ToListAsync();
    }
}
