using WebApplication1.Repository.DashboardRepository;
using WebApplication1.Utils.DTOs.DashboardDTO;

namespace WebApplication1.Services.DashboardServices;

/// <summary>
/// Service implementation for dashboard business logic.
/// Optimized with parallel queries and proper error handling.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _repository;
    private readonly ILogger<DashboardService> _logger;
    private const int DAYS_THRESHOLD_DOCUMENTS = 7;
    private const int TOP_TURE_COUNT = 5;
    private const int RECENT_LOGS_COUNT = 10;
    private const int REVENUE_DAYS = 30;

    public DashboardService(IDashboardRepository repository, ILogger<DashboardService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Gets complete dashboard overview with parallel queries for optimal performance.
    /// </summary>
    public async Task<DashboardOverviewDto> GetDashboardOverviewAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Create cancellation token source with timeout to prevent hanging
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(30)); // 30s timeout for all operations

            // Execute all independent queries in parallel for better performance
            var kpiTask = GetKPIDataAsync(cts.Token);
            var tureStatusTask = GetTureStatusDistribucijaAsync(cts.Token);
            var naloziStatusTask = GetNaloziStatusDistribucijaAsync(cts.Token);
            var prihod30DanaTask = GetPrihod30DanaAsync(cts.Token);
            var topTureTask = GetTopTureAsync(cts.Token);
            var kriticnaVozilaTask = GetKriticnaVozilaAsync(cts.Token);
            var logoviTask = GetNajnovijiLogoviAsync(cts.Token);

            // Wait for all tasks to complete (parallel execution)
            await Task.WhenAll(
                kpiTask,
                tureStatusTask,
                naloziStatusTask,
                prihod30DanaTask,
                topTureTask,
                kriticnaVozilaTask,
                logoviTask
            );

            // Build result
            return new DashboardOverviewDto
            {
                Kpi = await kpiTask,
                TureStatusDistribucija = await tureStatusTask,
                NaloziStatusDistribucija = await naloziStatusTask,
                Prihod30Dana = await prihod30DanaTask,
                TopTure = await topTureTask,
                KriticnaVozila = await kriticnaVozilaTask,
                NajnovijiLogovi = await logoviTask
            };
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Dashboard data fetch was cancelled or timed out");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dashboard overview data");
            throw;
        }
    }

    private async Task<KPIDto> GetKPIDataAsync(CancellationToken cancellationToken)
    {
        // Execute count queries in parallel
        var ukupnoTureTask = _repository.GetTotalTureCountAsync();
        var aktivneTureTask = _repository.GetAktivneTureCountAsync();
        var ukupnoNaloziTask = _repository.GetTotalNaloziCountAsync();
        var aktivniNaloziTask = _repository.GetAktivniNaloziCountAsync();
        var danasnjiPrihodTask = _repository.GetPrihodZaDanasAsync();
        var ukupnoVozilaTask = _repository.GetTotalVozilaCountAsync();
        var aktivnaVozilaTask = _repository.GetAktivnaVozilaCountAsync();
        var aktivniKlijentiTask = _repository.GetAktivniKlijentiCountAsync();
        var aktivniPrevozniciTask = _repository.GetAktivniPrevozniciCountAsync();

        // Wait for all count queries
        await Task.WhenAll(
            ukupnoTureTask,
            aktivneTureTask,
            ukupnoNaloziTask,
            aktivniNaloziTask,
            danasnjiPrihodTask,
            ukupnoVozilaTask,
            aktivnaVozilaTask,
            aktivniKlijentiTask,
            aktivniPrevozniciTask
        );

        // Calculate revenue for last 30 days
        var startDate = DateTime.UtcNow.AddDays(-REVENUE_DAYS).Date;
        var endDate = DateTime.UtcNow.Date;
        var prihod30Dana = await _repository.GetPrihodZaPeriodAsync(startDate, endDate);

        // Get vehicles with expiring documents
        var kriticnaVozila = await _repository.GetVozilaSaIsticucimDokumentimaAsync(
            DAYS_THRESHOLD_DOCUMENTS, 
            cancellationToken);

        return new KPIDto
        {
            UkupnoTura = await ukupnoTureTask,
            AktivneTure = await aktivneTureTask,
            UkupnoNalozi = await ukupnoNaloziTask,
            AktivniNalozi = await aktivniNaloziTask,
            DanasnjiPrihod = await danasnjiPrihodTask,
            Prihod30Dana = prihod30Dana,
            UkupnoVozila = await ukupnoVozilaTask,
            AktivnaVozila = await aktivnaVozilaTask,
            VozilaSaIsticucimDokumentima = kriticnaVozila.Count,
            AktivniKlijenti = await aktivniKlijentiTask,
            AktivniPrevoznici = await aktivniPrevozniciTask
        };
    }

    private async Task<List<StatusDistribucijaDto>> GetTureStatusDistribucijaAsync(CancellationToken cancellationToken)
    {
        var distribucija = await _repository.GetTureStatusDistribucijaAsync();
        
        return distribucija
            .Select(d => new StatusDistribucijaDto
            {
                Status = d.Key,
                Broj = d.Value
            })
            .OrderByDescending(d => d.Broj)
            .ToList();
    }

    private async Task<List<StatusDistribucijaDto>> GetNaloziStatusDistribucijaAsync(CancellationToken cancellationToken)
    {
        var distribucija = await _repository.GetNaloziStatusDistribucijaAsync();
        
        return distribucija
            .Select(d => new StatusDistribucijaDto
            {
                Status = d.Key,
                Broj = d.Value
            })
            .OrderByDescending(d => d.Broj)
            .ToList();
    }

    private async Task<List<Prihod30DanaDto>> GetPrihod30DanaAsync(CancellationToken cancellationToken)
    {
        var startDate = DateTime.UtcNow.AddDays(-REVENUE_DAYS).Date;
        var endDate = DateTime.UtcNow.Date;
        
        var prihodByDate = await _repository.GetPrihodByDateAsync(startDate, endDate);
        
        // Create dictionary for fast lookup
        var prihodDict = prihodByDate.ToDictionary(p => p.Datum, p => p.Suma);
        
        // Fill all days in range, even if no revenue (set to 0)
        var result = new List<Prihod30DanaDto>();
        for (var datum = startDate; datum <= endDate; datum = datum.AddDays(1))
        {
            result.Add(new Prihod30DanaDto
            {
                Datum = datum,
                Suma = prihodDict.GetValueOrDefault(datum, 0)
            });
        }
        
        return result;
    }

    private async Task<List<TopTuraDto>> GetTopTureAsync(CancellationToken cancellationToken)
    {
        var topTure = await _repository.GetTopTureByPriceAsync(TOP_TURE_COUNT, cancellationToken);
        
        return topTure.Select(t => new TopTuraDto
        {
            TuraId = t.TuraId,
            RedniBroj = t.RedniBroj ?? $"Tura-{t.TuraId}",
            Relacija = $"{t.MestoUtovara} - {t.MestoIstovara}",
            UlaznaCena = t.UlaznaCena ?? 0,
            PrevoznikNaziv = t.Prevoznik?.Naziv ?? "N/A",
            VoziloNaziv = t.Vozilo?.Naziv,
            KlijentNaziv = t.Klijent?.NazivFirme,
            StatusTure = t.StatusTure
        }).ToList();
    }

    private async Task<List<KriticnoVoziloDto>> GetKriticnaVozilaAsync(CancellationToken cancellationToken)
    {
        var vozila = await _repository.GetVozilaSaIsticucimDokumentimaAsync(
            DAYS_THRESHOLD_DOCUMENTS, 
            cancellationToken);
        
        var danas = DateTime.UtcNow.Date;
        var thresholdDate = danas.AddDays(DAYS_THRESHOLD_DOCUMENTS);
        var kriticna = new List<KriticnoVoziloDto>();

        foreach (var vozilo in vozila)
        {
            // Check registracija
            if (vozilo.RegistracijaDatumIsteka.HasValue &&
                vozilo.RegistracijaDatumIsteka.Value.Date >= danas &&
                vozilo.RegistracijaDatumIsteka.Value.Date <= thresholdDate)
            {
                kriticna.Add(new KriticnoVoziloDto
                {
                    VoziloId = vozilo.VoziloId,
                    Naziv = vozilo.Naziv,
                    TipProblema = "Registracija",
                    DatumIsteka = vozilo.RegistracijaDatumIsteka,
                    DanaDoIsteka = (int)(vozilo.RegistracijaDatumIsteka.Value.Date - danas).TotalDays
                });
            }

            // Check tehnički pregled
            if (vozilo.TehnickiPregledDatumIsteka.HasValue &&
                vozilo.TehnickiPregledDatumIsteka.Value.Date >= danas &&
                vozilo.TehnickiPregledDatumIsteka.Value.Date <= thresholdDate)
            {
                kriticna.Add(new KriticnoVoziloDto
                {
                    VoziloId = vozilo.VoziloId,
                    Naziv = vozilo.Naziv,
                    TipProblema = "Tehnički pregled",
                    DatumIsteka = vozilo.TehnickiPregledDatumIsteka,
                    DanaDoIsteka = (int)(vozilo.TehnickiPregledDatumIsteka.Value.Date - danas).TotalDays
                });
            }

            // Check PP Aparat
            if (vozilo.PPAparatDatumIsteka.HasValue &&
                vozilo.PPAparatDatumIsteka.Value.Date >= danas &&
                vozilo.PPAparatDatumIsteka.Value.Date <= thresholdDate)
            {
                kriticna.Add(new KriticnoVoziloDto
                {
                    VoziloId = vozilo.VoziloId,
                    Naziv = vozilo.Naziv,
                    TipProblema = "PP Aparat",
                    DatumIsteka = vozilo.PPAparatDatumIsteka,
                    DanaDoIsteka = (int)(vozilo.PPAparatDatumIsteka.Value.Date - danas).TotalDays
                });
            }

            // Check vinjete
            foreach (var vinjeta in vozilo.Vinjete.Where(v =>
                v.DatumIsteka.Date >= danas && v.DatumIsteka.Date <= thresholdDate))
            {
                kriticna.Add(new KriticnoVoziloDto
                {
                    VoziloId = vozilo.VoziloId,
                    Naziv = vozilo.Naziv,
                    TipProblema = "Vinjeta",
                    DatumIsteka = vinjeta.DatumIsteka,
                    DanaDoIsteka = (int)(vinjeta.DatumIsteka.Date - danas).TotalDays,
                    Detalji = $"{vinjeta.DrzavaKod} ({vinjeta.Drzava ?? ""})"
                });
            }
        }

        return kriticna.OrderBy(k => k.DanaDoIsteka).ThenBy(k => k.VoziloId).ToList();
    }

    private async Task<List<LogDto>> GetNajnovijiLogoviAsync(CancellationToken cancellationToken)
    {
        var logovi = await _repository.GetNajnovijiLogoviAsync(RECENT_LOGS_COUNT, cancellationToken);
        
        return logovi.Select(l => new LogDto
        {
            LogId = l.LogId,
            HappenedAtDate = l.HappenedAtDate,
            Process = l.Process,
            Activity = l.Activity,
            Message = l.Message,
            UserName = l.User?.Username
        }).ToList();
    }
}
