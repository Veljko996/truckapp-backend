using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using WebApplication1.DataAccess;
using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.PrevozniciRepository;

public class PrevozniciRepository : IPrevozniciRepository
{
    private readonly TruckContext _context;
    public PrevozniciRepository(TruckContext context)
    {
        _context = context;
    }

    public IQueryable<Prevoznik> GetAll()
    {
        return _context
            .Prevoznici!
            .AsNoTracking();
    }

    public async Task<Prevoznik?> GetById(int prevoznikId)
    {
        return await _context.Prevoznici!
            .FirstOrDefaultAsync(x => x.PrevoznikId == prevoznikId);
    }

    public void Create(Prevoznik prevoznik)
    {
        _context.Prevoznici!.Add(prevoznik);
    }

    public void Delete(Prevoznik prevoznik)
    {
        _context.Prevoznici!.Remove(prevoznik);
    }

    public void Update(Prevoznik prevoznik)
    {
        _context.Prevoznici.Update(prevoznik);
    }

    public async Task<bool> SaveChangesAsync()
    {
        try
        {
            return await _context.SaveChangesAsync() > 0;
        }
        catch (DbUpdateException ex)
        {
            // Re-throw with context for proper error handling
            _context.ChangeTracker.Clear();
            throw new InvalidOperationException("Database update failed. See inner exception for details.", ex);
        }
    }
}

