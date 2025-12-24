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
        return _context.Klijenti!.AsNoTracking().OrderBy(k => k.NazivFirme);
    }

    public async Task<Klijent?> GetById(int klijentId)
    {
        return await _context.Klijenti!.FirstOrDefaultAsync(x => x.KlijentId == klijentId);
    }

    public void Create(Klijent klijent)
    {
        _context.Klijenti!.Add(klijent);
    }

    public void Update(Klijent klijent)
    {
        _context.Klijenti!.Update(klijent);
    }

    public void Delete(Klijent klijent)
    {
        _context.Klijenti!.Remove(klijent);
    }

    public async Task<bool> SaveChangesAsync()
    {
        try
        {
            return await _context.SaveChangesAsync() > 0;
        }
        catch (DbUpdateException ex)
        {
            _context.ChangeTracker.Clear();
            throw new InvalidOperationException("Database update failed. See inner exception for details.", ex);
        }
    }
}

