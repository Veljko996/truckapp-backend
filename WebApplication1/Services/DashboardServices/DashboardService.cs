using WebApplication1.Repository.DashboardRepository;
using WebApplication1.Utils.DTOs.DashboardDTO;

namespace WebApplication1.Services.DashboardServices;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _repository;
    private readonly ILogger<DashboardService> _logger;
    private const int DAYS_THRESHOLD_DOCUMENTS = 7;
    private const int TOP_TURE_COUNT = 5;

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
            var topTure = await GetTopTureAsync(cancellationToken);
            var kriticnaVozila = await GetKriticnaVozilaAsync(cancellationToken);
            var topCarriers = await GetTop5CarriersAsync(cancellationToken);
            var topClients = await GetTop5ClientsByProfitAsync(cancellationToken);
            var lateUnloadNalogs = await GetLateUnloadNalogsAsync(cancellationToken);

            return new DashboardOverviewDto
            {
                Kpi = kpi,
                TopTure = topTure,
                KriticnaVozila = kriticnaVozila,
                TopCarriers = topCarriers,
                TopClients = topClients,
                LateUnloadNalogs = lateUnloadNalogs
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
        var kriticnaVozila = await _repository.GetVozilaSaIsticucimDokumentimaAsync(DAYS_THRESHOLD_DOCUMENTS, cancellationToken);
        var profit = await _repository.GetProfitLast30DaysAsync();

        return new KPIDto
        {
            UkupnoTura = await _repository.GetTotalTureCountAsync(),
            AktivneTure = await _repository.GetAktivneTureCountAsync(),
            TotalNalogsCount = await _repository.GetTotalNaloziCountAsync(),
            AktivniNalozi = await _repository.GetAktivniNaloziCountAsync(),
            UkupnoVozila = await _repository.GetTotalVozilaCountAsync(),
            AktivnaVozila = await _repository.GetAktivnaVozilaCountAsync(),
            VozilaSaIsticucimDokumentima = kriticnaVozila.Count,
            AktivniKlijenti = await _repository.GetAktivniKlijentiCountAsync(),
            AktivniPrevoznici = await _repository.GetAktivniPrevozniciCountAsync(),
            ActiveNalogsCount = await _repository.GetActiveNalogsCountAsync(),
            LateUnloadNalogsCount = await _repository.GetLateUnloadNalogsCountAsync(),
            ProfitLast30DaysEUR = profit.ProfitEUR,
            ProfitLast30DaysRSD = profit.ProfitRSD
        };
    }


    private async Task<List<LateUnloadNalogDto>> GetLateUnloadNalogsAsync(CancellationToken cancellationToken)
    {
        var nalogs = await _repository.GetLateUnloadNalogsAsync(cancellationToken);
        return nalogs.Select(n => new LateUnloadNalogDto
        {
            NalogId = n.NalogId,
            NalogBroj = n.NalogBroj ?? $"Nalog-{n.NalogId}",
            ClientName = n.Tura?.Klijent?.NazivFirme ?? "N/A",
            PlannedUnloadDate = n.DatumIstovara,
            Status = n.StatusNaloga ?? "Nepoznat"
        }).ToList();
    }

    private async Task<List<TopTuraDto>> GetTopTureAsync(CancellationToken cancellationToken)
    {
        var topTure = await _repository.GetTopTureByProfitAsync(TOP_TURE_COUNT, cancellationToken);
        return topTure.Select(t =>
        {
            var profit = (t.IzlaznaCena ?? 0) - (t.UlaznaCena ?? 0);
            var valuta = t.Valuta ?? "RSD";

            return new TopTuraDto
            {
                TuraId = t.TuraId,
                RedniBroj = t.RedniBroj ?? $"Tura-{t.TuraId}",
                Relacija = $"{t.MestoUtovara} - {t.MestoIstovara}",
                ProfitEUR = valuta == "EUR" ? profit : null,
                ProfitRSD = valuta == "RSD" || valuta == null ? profit : null
            };
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

    private async Task<List<TopCarrierDto>> GetTop5CarriersAsync(CancellationToken cancellationToken)
    {
        var carriers = await _repository.GetTop5CarriersLast30DaysAsync(cancellationToken);
        return carriers.Select(c => new TopCarrierDto
        {
            CarrierId = c.PrevoznikId,
            CarrierName = c.Naziv,
            TotalToursCount = c.TotalToursCount
        }).ToList();
    }

    private async Task<List<TopClientDto>> GetTop5ClientsByProfitAsync(CancellationToken cancellationToken)
    {
        var clients = await _repository.GetTop5ClientsByProfitLast30DaysAsync(cancellationToken);
        return clients.Select(c => new TopClientDto
        {
            ClientId = c.KlijentId,
            ClientName = c.NazivFirme,
            TotalProfitEUR = c.ProfitEUR,
            TotalProfitRSD = c.ProfitRSD
        }).ToList();
    }
}
