using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;
using WebApplication1.Utils.Tenant;

public class TureRepository : ITureRepository
{
    private readonly TruckContext _context;
    private readonly ITenantProvider _tenantProvider;

    public TureRepository(TruckContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
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

    public async Task<string> GetNextTuraBrojAsync()
    {
        return await GetNextDocumentNumberAsync("TURA");
    }

    public async Task<string> GetNextDocumentNumberAsync(string documentType)
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
            new SqlParameter("@DocumentType", documentType),
            new SqlParameter("@TenantId", _tenantProvider.CurrentTenantId),
            output
        );

        return (string)output.Value!;
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
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

    public async Task<Prevoznik?> GetPrevoznikByIdAsync(int id)
    {
        return await _context.Prevoznici.FirstOrDefaultAsync(p => p.PrevoznikId == id);
    }

    public async Task<NasaVozila?> GetVoziloByIdAsync(int id)
    {
        return await _context.NasaVozila.FirstOrDefaultAsync(v => v.VoziloId == id);
    }

    public async Task<bool> IsVoziloZauzetoNaNaloguAsync(int voziloId, int? excludeTuraId = null)
    {
        var activeStatuses = new[] { "Istovaren", "Završen", "Storniran", "Ponisten" };
        return await _context.Nalozi
            .AnyAsync(n =>
                n.Tura != null
                && n.Tura.VoziloId == voziloId
                && !activeStatuses.Contains(n.StatusNaloga ?? "")
                && (excludeTuraId == null || n.TuraId != excludeTuraId.Value));
    }
}
