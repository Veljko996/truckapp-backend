using Microsoft.EntityFrameworkCore;
using WebApplication1.DataAccess;
using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.VrstaNadogradnjeRepository;

public class VrstaNadogradnjeRepository : IVrstaNadogradnjeRepository
{
    private readonly TruckContext _context;

    public VrstaNadogradnjeRepository(TruckContext context)
    {
        _context = context;
    }

    public IQueryable<VrstaNadogradnje> GetAll()
    {
        return _context
            .VrsteNadogradnje!
            .AsNoTracking()
            .OrderBy(v => v.Naziv);
    }

    public async Task<VrstaNadogradnje?> GetById(int vrstaNadogradnjeId)
    {
        return await _context.VrsteNadogradnje!
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.VrstaNadogradnjeId == vrstaNadogradnjeId);
    }
}

