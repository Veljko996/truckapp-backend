using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DataAccess;
using WebApplication1.DataAccess.Models;
using WebApplication1.Utils.DTOs.DashboardDTO;
using WebApplication1.Utils.Tenant;

namespace WebApplication1.Repository.DashboardRepository;

public class DashboardRepository : IDashboardRepository
{
    private readonly TruckContext _context;
    private readonly ITenantProvider _tenantProvider;

    public DashboardRepository(TruckContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<DashboardStatsDto> GetExternalDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        var tenantParam = new SqlParameter("@TenantId", _tenantProvider.CurrentTenantId);

        var list = await _context.Database
            .SqlQueryRaw<ExternalDashboardStatsRawDto>("EXEC dbo.GetExternalDashboardStats @TenantId", tenantParam)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var raw = list.FirstOrDefault();
        if (raw == null)
            return new DashboardStatsDto();

        return new DashboardStatsDto
        {
            UkupanBrojNaloga = raw.UkupanBrojNaloga ?? 0,
            UkupanBrojIzvrsenihNaloga = raw.UkupanBrojIzvrsenihNaloga ?? 0,
            BrojAktivnihNaloga = raw.BrojAktivnihNaloga ?? 0,
            BrojNalogaDanas = raw.BrojNalogaDanas ?? 0,
            BrojNalogaNedelja = raw.BrojNalogaNedelja ?? 0,
            BrojNalogaMesec = raw.BrojNalogaMesec ?? 0,
            ProfitDanasEUR = raw.ProfitDanasEUR ?? 0m,
            ProfitDanasRSD = raw.ProfitDanasRSD ?? 0m,
            ProfitNedeljaEUR = raw.ProfitNedeljaEUR ?? 0m,
            ProfitNedeljaRSD = raw.ProfitNedeljaRSD ?? 0m,
            ProfitMesecEUR = raw.ProfitMesecEUR ?? 0m,
            ProfitMesecRSD = raw.ProfitMesecRSD ?? 0m
        };
    }

    public async Task<List<DashboardMonthlyProfitDto>> GetMonthlyProfitAsync(int monthsBack = 12, CancellationToken cancellationToken = default)
    {
        var monthsParam = new SqlParameter("@MonthsBack", monthsBack);
        var tenantParam = new SqlParameter("@TenantId", _tenantProvider.CurrentTenantId);

        return await _context.Database
            .SqlQueryRaw<DashboardMonthlyProfitDto>("EXEC dbo.GetDashboardMonthlyProfit @MonthsBack, @TenantId", monthsParam, tenantParam)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<NasaVozila>> GetVozilaSaIsticucimDokumentimaAsync(int daysThreshold, CancellationToken cancellationToken = default)
    {
        var danas = DateTime.UtcNow.Date;
        var thresholdDate = danas.AddDays(daysThreshold);

        return await _context.NasaVozila
            .Include(v => v.Vinjete)
            .Where(v =>
                // Registracija
                (v.RegistracijaDatumIsteka.HasValue
                    && v.RegistracijaDatumIsteka.Value.Date <= thresholdDate) ||
                // Tehnički pregled
                (v.TehnickiPregledDatumIsteka.HasValue
                    && v.TehnickiPregledDatumIsteka.Value.Date <= thresholdDate) ||
                // PP Aparat
                (v.PPAparatDatumIsteka.HasValue
                    && v.PPAparatDatumIsteka.Value.Date <= thresholdDate) ||
                // Vinjete
                v.Vinjete.Any(vin =>
                    vin.DatumIsteka.Date <= thresholdDate))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
