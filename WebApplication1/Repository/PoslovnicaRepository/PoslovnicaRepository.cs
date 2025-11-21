using Microsoft.EntityFrameworkCore;
using WebApplication1.DataAccess;
using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.PoslovnicaRepository;

public class PoslovnicaRepository : IPoslovnicaRepository
{
    private readonly TruckContext _context;

    public PoslovnicaRepository(TruckContext context)
    {
        _context = context;
    }

    public async Task<Poslovnica?> GetByIdAsync(int poslovnicaId)
    {
        return await _context.Poslovnice
            .FirstOrDefaultAsync(p => p.PoslovnicaId == poslovnicaId);
    }

    public async Task<Poslovnica?> GetByIdWithEmployeesAsync(int poslovnicaId)
    {
        return await _context.Poslovnice
            .Include(p => p.Employees)
                .ThenInclude(e => e.User)
            .FirstOrDefaultAsync(p => p.PoslovnicaId == poslovnicaId);
    }

    public async Task<IEnumerable<Poslovnica>> GetAllAsync()
    {
        return await _context.Poslovnice
            .OrderBy(p => p.PJ)
            .ToListAsync();
    }

    public async Task<bool> PJExistsAsync(string pj)
    {
        if (string.IsNullOrWhiteSpace(pj))
            return false;

        return await _context.Poslovnice.AnyAsync(p => p.PJ == pj);
    }

    public async Task<bool> PJExistsForOtherPoslovnicaAsync(string pj, int excludePoslovnicaId)
    {
        if (string.IsNullOrWhiteSpace(pj))
            return false;

        return await _context.Poslovnice
            .AnyAsync(p => p.PJ == pj && p.PoslovnicaId != excludePoslovnicaId);
    }

    public async Task AddAsync(Poslovnica poslovnica)
    {
        await _context.Poslovnice.AddAsync(poslovnica);
    }

    public async Task UpdateAsync(Poslovnica poslovnica)
    {
        _context.Poslovnice.Update(poslovnica);
    }

    public async Task DeleteAsync(Poslovnica poslovnica)
    {
        _context.Poslovnice.Remove(poslovnica);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}

