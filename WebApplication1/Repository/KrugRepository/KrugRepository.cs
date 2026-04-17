using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DataAccess;
using WebApplication1.DataAccess.Models;
using WebApplication1.Utils.Tenant;

namespace WebApplication1.Repository.KrugRepository;

public class KrugRepository : IKrugRepository
{
    private readonly TruckContext _context;
    private readonly ITenantProvider _tenantProvider;

    public KrugRepository(TruckContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public IQueryable<Krug> GetAll()
    {
        return _context.Krugovi
            .AsNoTracking()
            .Include(k => k.Vozilo)
            .Include(k => k.Ture)
            .OrderByDescending(k => k.KrugId);
    }

    public async Task<Krug?> GetByIdAsync(int id)
    {
        return await _context.Krugovi
            .Include(k => k.Vozilo)
            .FirstOrDefaultAsync(k => k.KrugId == id);
    }

    public async Task<Krug?> GetByIdWithTureAsync(int id)
    {
        return await _context.Krugovi
            .Include(k => k.Vozilo)
            .Include(k => k.Ture)
                .ThenInclude(t => t.Klijent)
            .Include(k => k.Ture)
                .ThenInclude(t => t.Prevoznik)
            .Include(k => k.Troskovi)
                .ThenInclude(t => t.TipTroska)
            .FirstOrDefaultAsync(k => k.KrugId == id);
    }

    public async Task<Krug?> GetOpenByVoziloIdAsync(int voziloId)
    {
        return await _context.Krugovi
            .FirstOrDefaultAsync(k => k.VoziloId == voziloId && k.Status == "Otvoren");
    }

    public async Task<string> GetNextKrugBrojAsync()
    {
        var output = new SqlParameter
        {
            ParameterName = "@Result",
            SqlDbType = SqlDbType.NVarChar,
            Size = 20,
            Direction = ParameterDirection.Output
        };

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC dbo.GetNextDocumentNumber @DocumentType, @TenantId, @Result OUTPUT",
            new SqlParameter("@DocumentType", "KRUG"),
            new SqlParameter("@TenantId", _tenantProvider.CurrentTenantId),
            output
        );

        return (string)output.Value!;
    }

    public void Add(Krug entity)
    {
        _context.Krugovi.Add(entity);
    }

    public void Update(Krug entity)
    {
        _context.Krugovi.Update(entity);
    }

    public void Delete(Krug entity)
    {
        _context.Krugovi.Remove(entity);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}
