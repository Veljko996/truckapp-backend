using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace WebApplication1.Repository.VinjeteRepository;

public class VinjeteRepository : IVinjeteRepository
{
    private readonly TruckContext _context;
    public VinjeteRepository(TruckContext context)
    {
        _context = context;
    }

    public IQueryable<Vinjeta> GetAll()
    {
        return _context
            .Vinjete!
            .Include(v => v.Vozilo)
            .AsNoTracking();
    }

    public async Task<Vinjeta?> GetById(int id)
    {
        return await _context.Vinjete
            .Include(v => v.Vozilo)
            .FirstOrDefaultAsync(v => v.VinjetaId == id);
    }

    public void Create(Vinjeta vinjeta)
    {
        _context.Vinjete!.Add(vinjeta);
    }

    public void Update(Vinjeta vinjeta) 
    {
        _context.Vinjete!.Update(vinjeta);
    }

    public void Delete(Vinjeta vinjeta)
    {
        _context.Vinjete!.Remove(vinjeta);

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

    public async Task<bool> VehicleExistsAsync(int voziloId)
    {
        return await _context.NasaVozila
            .AnyAsync(v => v.VoziloId == voziloId);
    }

    public async Task<Vinjeta?> GetActiveVignetteForVehicleAsync(int voziloId, string drzavaKod, DateTime datumPocetka, DateTime datumIsteka, int? excludeVinjetaId = null)
    {
        var query = _context.Vinjete
            .Where(v => v.VoziloId == voziloId &&
                v.DrzavaKod == drzavaKod &&
                // Check for date overlap
                v.DatumPocetka <= datumIsteka &&
                v.DatumIsteka >= datumPocetka);

        if (excludeVinjetaId.HasValue)
        {
            query = query.Where(v => v.VinjetaId != excludeVinjetaId.Value);
        }

        return await query.FirstOrDefaultAsync();
    }

}
