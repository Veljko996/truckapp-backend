using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using WebApplication1.DataAccess;
using WebApplication1.DataAccess.Models;
using WebApplication1.Utils.Enums;

namespace WebApplication1.Repository.TureRepository;

public class TureRepository : ITureRepository
{
    private readonly TruckContext _context;

    public TureRepository(TruckContext context)
    {
        _context = context;
    }

    public IQueryable<Tura> GetAll()
    {
        return _context.Ture
            .Include(t => t.Vozilo)
            .Include(t => t.Prevoznik)
            .AsNoTracking()
            .OrderByDescending(t => t.UtovarDatum);
    }

    public async Task<Tura?> GetByIdAsync(int id)
    {
        return await _context.Ture
            .Include(t => t.Vozilo)
            .Include(t => t.Prevoznik)
            .Include(t => t.StatusLogovi.OrderByDescending(sl => sl.Vreme))
            .FirstOrDefaultAsync(t => t.TuraId == id);
    }

    public void Create(Tura tura)
    {
        _context.Ture.Add(tura);
    }

    public void Update(Tura tura)
    {
        _context.Ture.Update(tura);
    }

    public void Delete(Tura tura)
    {
        _context.Ture.Remove(tura);
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
            throw new InvalidOperationException("Database update failed. See inner exception for details.", ex);
        }
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _context.Database.BeginTransactionAsync();
    }

    /// <summary>
    /// Checks if a vehicle is available for assignment. Uses database query to prevent race conditions.
    /// </summary>
    public async Task<bool> IsVehicleAvailableAsync(int voziloId, int? excludeTuraId = null)
    {
        // Use AsNoTracking for read-only check to improve performance
        var query = _context.Ture
            .AsNoTracking()
            .Where(t => t.VoziloId == voziloId &&
                t.StatusTrenutni != TuraStatus.Zavrseno &&
                t.StatusTrenutni != TuraStatus.Otkazano);

        if (excludeTuraId.HasValue)
        {
            query = query.Where(t => t.TuraId != excludeTuraId.Value);
        }

        return !await query.AnyAsync();
    }

    public async Task<bool> PrevoznikExistsAsync(int prevoznikId)
    {
        return await _context.Prevoznici
            .AnyAsync(p => p.PrevoznikId == prevoznikId);
    }

    public async Task<bool> VoziloExistsAsync(int voziloId)
    {
        return await _context.NasaVozila
            .AnyAsync(v => v.VoziloId == voziloId);
    }

    public async Task AddStatusLogAsync(TuraStatusLog statusLog)
    {
        await _context.TuraStatusLogs.AddAsync(statusLog);
    }
}
