namespace WebApplication1.Repository.NalogRepository;

public class NalogRepository : INalogRepository
{
    private readonly TruckContext _context;

    public NalogRepository(TruckContext context)
    {
        _context = context;
    }

    public async Task<Nalog?> GetByIdAsync(int id)
    {
        return await _context
            .Nalozi
            .Include(n => n.Prevoznik)
            .Include(n => n.Tura)
            .FirstOrDefaultAsync(n => n.NalogId == id);
    }

    public IQueryable<Nalog> GetAll()
    {
        return _context
            .Nalozi
            .AsNoTracking()
            .Include(n => n.Prevoznik)
            .Include(n => n.Tura)
            .OrderByDescending(n => n.NalogId);
    }

    public void Add(Nalog nalog)
    {
        _context.Nalozi.Add(nalog);
    }

    public void Update(Nalog nalog)
    {
        _context
            .Nalozi
            .Update(nalog);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}

