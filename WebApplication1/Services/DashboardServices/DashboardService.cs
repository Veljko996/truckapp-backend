using WebApplication1.Repository.DashboardRepository;
using WebApplication1.Utils.DTOs.DashboardDTO;

namespace WebApplication1.Services.DashboardServices;

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

    public async Task<DashboardOverviewDto> GetDashboardOverviewAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var kpi = await GetKPIDataAsync(cancellationToken);
            var tureStatus = await GetTureStatusDistribucijaAsync(cancellationToken);
            var naloziStatus = await GetNaloziStatusDistribucijaAsync(cancellationToken);
            var prihod30Dana = await GetPrihod30DanaAsync(cancellationToken);
            var topTure = await GetTopTureAsync(cancellationToken);
            var kriticnaVozila = await GetKriticnaVozilaAsync(cancellationToken);
            var logovi = await GetNajnovijiLogoviAsync(cancellationToken);

            return new DashboardOverviewDto
            {
                Kpi = kpi,
                TureStatusDistribucija = tureStatus,
                NaloziStatusDistribucija = naloziStatus,
                Prihod30Dana = prihod30Dana,
                TopTure = topTure,
                KriticnaVozila = kriticnaVozila,
                NajnovijiLogovi = logovi
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dashboard overview data");
            throw;
        }
    }

    private async Task<KPIDto> GetKPIDataAsync(CancellationToken cancellationToken)
    {
        var startDate = DateTime.UtcNow.AddDays(-REVENUE_DAYS).Date;
        var endDate = DateTime.UtcNow.Date;
        var kriticnaVozila = await _repository.GetVozilaSaIsticucimDokumentimaAsync(DAYS_THRESHOLD_DOCUMENTS, cancellationToken);

        return new KPIDto
        {
            UkupnoTura = await _repository.GetTotalTureCountAsync(),
            AktivneTure = await _repository.GetAktivneTureCountAsync(),
            UkupnoNalozi = await _repository.GetTotalNaloziCountAsync(),
            AktivniNalozi = await _repository.GetAktivniNaloziCountAsync(),
            DanasnjiPrihod = await _repository.GetPrihodZaDanasAsync(),
            Prihod30Dana = await _repository.GetPrihodZaPeriodAsync(startDate, endDate),
            UkupnoVozila = await _repository.GetTotalVozilaCountAsync(),
            AktivnaVozila = await _repository.GetAktivnaVozilaCountAsync(),
            VozilaSaIsticucimDokumentima = kriticnaVozila.Count,
            AktivniKlijenti = await _repository.GetAktivniKlijentiCountAsync(),
            AktivniPrevoznici = await _repository.GetAktivniPrevozniciCountAsync()
        };
    }

    private async Task<List<StatusDistribucijaDto>> GetTureStatusDistribucijaAsync(CancellationToken cancellationToken)
    {
        var distribucija = await _repository.GetTureStatusDistribucijaAsync();
        return distribucija.Select(d => new StatusDistribucijaDto { Status = d.Key, Broj = d.Value })
            .OrderByDescending(d => d.Broj).ToList();
    }

    private async Task<List<StatusDistribucijaDto>> GetNaloziStatusDistribucijaAsync(CancellationToken cancellationToken)
    {
        var distribucija = await _repository.GetNaloziStatusDistribucijaAsync();
        return distribucija.Select(d => new StatusDistribucijaDto { Status = d.Key, Broj = d.Value })
            .OrderByDescending(d => d.Broj).ToList();
    }

    private async Task<List<Prihod30DanaDto>> GetPrihod30DanaAsync(CancellationToken cancellationToken)
    {
        var startDate = DateTime.UtcNow.AddDays(-REVENUE_DAYS).Date;
        var endDate = DateTime.UtcNow.Date;
        var prihodByDate = await _repository.GetPrihodByDateAsync(startDate, endDate);
        var prihodDict = prihodByDate.ToDictionary(p => p.Datum, p => p.Suma);
        
        var result = new List<Prihod30DanaDto>();
        for (var datum = startDate; datum <= endDate; datum = datum.AddDays(1))
            result.Add(new Prihod30DanaDto { Datum = datum, Suma = prihodDict.GetValueOrDefault(datum, 0) });
        
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
        var vozila = await _repository.GetVozilaSaIsticucimDokumentimaAsync(DAYS_THRESHOLD_DOCUMENTS, cancellationToken);
        var danas = DateTime.UtcNow.Date;
        var thresholdDate = danas.AddDays(DAYS_THRESHOLD_DOCUMENTS);
        var kriticna = new List<KriticnoVoziloDto>();

        foreach (var vozilo in vozila)
        {
            if (vozilo.RegistracijaDatumIsteka.HasValue && vozilo.RegistracijaDatumIsteka.Value.Date >= danas && vozilo.RegistracijaDatumIsteka.Value.Date <= thresholdDate)
                kriticna.Add(new KriticnoVoziloDto { VoziloId = vozilo.VoziloId, Naziv = vozilo.Naziv, TipProblema = "Registracija", DatumIsteka = vozilo.RegistracijaDatumIsteka, DanaDoIsteka = (int)(vozilo.RegistracijaDatumIsteka.Value.Date - danas).TotalDays });

            if (vozilo.TehnickiPregledDatumIsteka.HasValue && vozilo.TehnickiPregledDatumIsteka.Value.Date >= danas && vozilo.TehnickiPregledDatumIsteka.Value.Date <= thresholdDate)
                kriticna.Add(new KriticnoVoziloDto { VoziloId = vozilo.VoziloId, Naziv = vozilo.Naziv, TipProblema = "Tehnički pregled", DatumIsteka = vozilo.TehnickiPregledDatumIsteka, DanaDoIsteka = (int)(vozilo.TehnickiPregledDatumIsteka.Value.Date - danas).TotalDays });

            if (vozilo.PPAparatDatumIsteka.HasValue && vozilo.PPAparatDatumIsteka.Value.Date >= danas && vozilo.PPAparatDatumIsteka.Value.Date <= thresholdDate)
                kriticna.Add(new KriticnoVoziloDto { VoziloId = vozilo.VoziloId, Naziv = vozilo.Naziv, TipProblema = "PP Aparat", DatumIsteka = vozilo.PPAparatDatumIsteka, DanaDoIsteka = (int)(vozilo.PPAparatDatumIsteka.Value.Date - danas).TotalDays });

            foreach (var vinjeta in vozilo.Vinjete.Where(v => v.DatumIsteka.Date >= danas && v.DatumIsteka.Date <= thresholdDate))
                kriticna.Add(new KriticnoVoziloDto { VoziloId = vozilo.VoziloId, Naziv = vozilo.Naziv, TipProblema = "Vinjeta", DatumIsteka = vinjeta.DatumIsteka, DanaDoIsteka = (int)(vinjeta.DatumIsteka.Date - danas).TotalDays, Detalji = $"{vinjeta.DrzavaKod} ({vinjeta.Drzava ?? ""})" });
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
