using Microsoft.EntityFrameworkCore;
using WebApplication1.DataAccess;
using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.KrugTroskoviRepository;

public class KrugTroskoviRepository : IKrugTroskoviRepository
{
    private readonly TruckContext _context;

    public KrugTroskoviRepository(TruckContext context)
    {
        _context = context;
    }

    public async Task<List<KrugTrosak>> GetByKrugIdAsync(int krugId)
    {
        return await _context.KrugTroskovi
            .Include(t => t.TipTroska)
            .Where(t => t.KrugId == krugId)
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<KrugTrosak?> GetByIdAsync(int krugTrosakId)
    {
        return await _context.KrugTroskovi
            .FirstOrDefaultAsync(t => t.KrugTrosakId == krugTrosakId);
    }

    public void Add(KrugTrosak entity)
    {
        _context.KrugTroskovi.Add(entity);
    }

    public void Delete(KrugTrosak entity)
    {
        _context.KrugTroskovi.Remove(entity);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}
