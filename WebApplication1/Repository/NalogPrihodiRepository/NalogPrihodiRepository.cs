using Microsoft.EntityFrameworkCore;
using WebApplication1.DataAccess;
using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.NalogPrihodiRepository;

public class NalogPrihodiRepository : INalogPrihodiRepository
{
    private readonly TruckContext _context;

    public NalogPrihodiRepository(TruckContext context)
    {
        _context = context;
    }

    public async Task<List<NalogPrihod>> GetByNalogIdAsync(int nalogId)
    {
        return await _context.NalogPrihodi
            .Where(p => p.NalogId == nalogId)
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<NalogPrihod?> GetByIdAsync(int prihodId)
    {
        return await _context.NalogPrihodi
            .FirstOrDefaultAsync(p => p.PrihodId == prihodId);
    }

    public async Task<NalogPrihod?> GetSeededInitialByNalogIdAsync(int nalogId)
    {
        return await _context.NalogPrihodi
            .FirstOrDefaultAsync(p => p.NalogId == nalogId && p.IsSeededInitial);
    }

    public async Task<bool> HasAnyByNalogIdAsync(int nalogId)
    {
        return await _context.NalogPrihodi
            .AnyAsync(p => p.NalogId == nalogId);
    }

    public void Add(NalogPrihod entity)
    {
        _context.NalogPrihodi.Add(entity);
    }

    public void Update(NalogPrihod entity)
    {
        _context.NalogPrihodi.Update(entity);
    }

    public void Delete(NalogPrihod entity)
    {
        _context.NalogPrihodi.Remove(entity);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}
