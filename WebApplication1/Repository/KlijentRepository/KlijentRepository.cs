using Microsoft.EntityFrameworkCore;
using WebApplication1.DataAccess;
using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.KlijentRepository;

public class KlijentRepository : IKlijentRepository
{
    private readonly TruckContext _context;

    public KlijentRepository(TruckContext context)
    {
        _context = context;
    }

    public IQueryable<Klijent> GetAll()
    {
        return _context
            .Klijenti!
            .AsNoTracking()
            .OrderBy(k => k.NazivFirme);
    }

    public async Task<Klijent?> GetById(int klijentId)
    {
        return await _context.Klijenti!
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.KlijentId == klijentId);
    }
}

