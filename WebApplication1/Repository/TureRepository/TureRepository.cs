using Microsoft.EntityFrameworkCore.Storage;

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
            .AsNoTracking()
            .Include(t => t.Prevoznik)
            .Include(t => t.Vozilo)
            .Include(t => t.Klijent)
            .Include(t => t.VrstaNadogradnje)
            .OrderByDescending(t => t.TuraId);
    }

    public async Task<Tura?> GetByIdAsync(int id)
    {
        return await _context.Ture
            .Include(t => t.Prevoznik)
            .Include(t => t.Vozilo)
            .Include(t => t.Klijent)
            .Include(t => t.VrstaNadogradnje)
            .FirstOrDefaultAsync(t => t.TuraId == id);
    }

    public void Add(Tura tura)
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
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _context.Database.BeginTransactionAsync();
    }

    public async Task<bool> PrevoznikExistsAsync(int prevoznikId)
    {
        return await _context.Prevoznici.AnyAsync(p => p.PrevoznikId == prevoznikId);
    }

    public async Task<bool> VoziloExistsAsync(int voziloId)
    {
        return await _context.NasaVozila.AnyAsync(v => v.VoziloId == voziloId);
    }

    public async Task<bool> KlijentExistsAsync(int klijentId)
    {
        return await _context.Klijenti.AnyAsync(c => c.KlijentId == klijentId);
    }
}
