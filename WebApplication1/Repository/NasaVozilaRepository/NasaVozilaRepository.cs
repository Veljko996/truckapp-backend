using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using WebApplication1.DataAccess;
using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.NasaVozilaRepository;

public class NasaVozilaRepository : INasaVozilaRepository
{
    private readonly TruckContext _context;
    public NasaVozilaRepository(TruckContext context)
    {
        _context = context;
    }

    public IQueryable<NasaVozila> GetAll()
    {
        return _context
            .NasaVozila!
            .Include(x => x.Vinjete)
            .AsNoTracking();
    }

    public async Task<NasaVozila?> GetById(int voziloId)
    {
        // Load related entities only when needed for business logic checks
        // Ture are loaded for delete validation, Vinjete for display
        return await _context.NasaVozila!
            .Include(x => x.Vinjete)
            .Include(x => x.Ture)
            .FirstOrDefaultAsync(x => x.VoziloId == voziloId);
    }

    public void Create(NasaVozila vozilo)
    {
        _context.NasaVozila!.Add(vozilo);
    }
    public void Delete(NasaVozila vozilo)
    {
        _context.NasaVozila!.Remove(vozilo);
    }

    public void Update(NasaVozila vozilo)
    {
        _context.NasaVozila.Update(vozilo);
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
