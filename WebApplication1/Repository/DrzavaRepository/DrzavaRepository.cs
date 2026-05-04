using Microsoft.EntityFrameworkCore;
using WebApplication1.DataAccess;
using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.DrzavaRepository;

public class DrzavaRepository : IDrzavaRepository
{
    private readonly TruckContext _context;

    public DrzavaRepository(TruckContext context)
    {
        _context = context;
    }

    public IQueryable<Drzava> GetAllActive()
    {
        return _context.Drzave
            .AsNoTracking()
            .Where(d => d.Aktivna)
            .OrderBy(d => d.Naziv);
    }

    public async Task<Drzava?> GetByIdAsync(int drzavaId)
    {
        return await _context.Drzave
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DrzavaId == drzavaId && d.Aktivna);
    }
}
