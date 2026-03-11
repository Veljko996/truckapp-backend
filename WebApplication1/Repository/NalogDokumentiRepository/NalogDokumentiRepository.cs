using Microsoft.EntityFrameworkCore;
using WebApplication1.DataAccess;
using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.NalogDokumentiRepository;

public class NalogDokumentiRepository : INalogDokumentiRepository
{
    private readonly TruckContext _context;

    public NalogDokumentiRepository(TruckContext context)
    {
        _context = context;
    }

    public async Task<List<NalogDokument>> GetByNalogIdAsync(int nalogId)
    {
        return await _context.NalogDokumenti
            .Include(d => d.TipDokumenta)
            .Where(d => d.NalogId == nalogId && !d.IsDeleted)
            .AsNoTracking()
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<NalogDokument?> GetByIdAsync(int dokumentId)
    {
        return await _context.NalogDokumenti
            .Include(d => d.TipDokumenta)
            .FirstOrDefaultAsync(d => d.DokumentId == dokumentId && !d.IsDeleted);
    }

    public void Add(NalogDokument entity)
    {
        _context.NalogDokumenti.Add(entity);
    }

    public void Update(NalogDokument entity)
    {
        _context.NalogDokumenti.Update(entity);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<List<TipDokumenta>> GetAllTipoviAsync()
    {
        return await _context.TipoviDokumenata
            .AsNoTracking()
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Naziv)
            .ToListAsync();
    }
}
