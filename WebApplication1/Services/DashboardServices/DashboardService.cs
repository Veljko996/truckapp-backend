using WebApplication1.Repository.DashboardRepository;
using WebApplication1.Utils.DTOs.DashboardDTO;
using WebApplication1.Utils.Enums;

namespace WebApplication1.Services.DashboardServices;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(IDashboardRepository dashboardRepository, ILogger<DashboardService> logger)
    {
        _dashboardRepository = dashboardRepository;
        _logger = logger;
    }

    public async Task<DashboardOverviewDto> GetDashboardOverviewAsync()
    {
        try
        {
            // Sekvencijalno učitavanje (DbContext nije thread-safe za paralelno korišćenje)
            var kpi = await GetKPIDataAsync();
            var statusDistribucija = await GetStatusDistribucijaAsync();
            var prihod30Dana = await GetPrihod30DanaAsync();
            var topTure = await GetTopTureAsync();
            var kriticnaVozila = await GetKriticnaVozilaAsync();
            var logovi = await GetLogoviAsync();

            return new DashboardOverviewDto
            {
                Kpi = kpi,
                StatusDistribucija = statusDistribucija,
                Prihod30Dana = prihod30Dana,
                TopTure = topTure,
                KriticnaVozila = kriticnaVozila,
                Logovi = logovi
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri dohvatanju dashboard podataka");
            throw;
        }
    }

    private async Task<KPIDto> GetKPIDataAsync()
    {
        // Sekvencijalno izvršavanje (DbContext nije thread-safe)
        var aktivneTure = await _dashboardRepository.GetAktivneTureCountAsync();
        var danasnjiPrihod = await _dashboardRepository.GetDanasnjiPrihodAsync();
        var aktivnaVozila = await _dashboardRepository.GetAktivnaVozilaCountAsync();
        var problematickeTure = await _dashboardRepository.GetProblematickeTureCountAsync();
        var vozilaSaIsticucim = await _dashboardRepository.GetVozilaSaIsticucimDokumentimaCountAsync(7);

        return new KPIDto
        {
            AktivneTure = aktivneTure,
            DanasnjiPrihod = danasnjiPrihod,
            BrojAktivnihVozila = aktivnaVozila,
            ProblematickeTure = problematickeTure,
            VozilaSaIsticucimDokumentima = vozilaSaIsticucim
        };
    }

    private async Task<List<StatusDistribucijaDto>> GetStatusDistribucijaAsync()
    {
        var distribucija = await _dashboardRepository.GetStatusDistribucijaAsync();
        
        // Filtrirati samo relevantne statuse za prikaz
        var relevantniStatusi = new[] 
        { 
            TuraStatus.UPripremi, 
            TuraStatus.NaPutu, 
            TuraStatus.Zavrseno, 
            TuraStatus.Zakasnjenje 
        };

        return distribucija
            .Where(d => relevantniStatusi.Contains(d.Status))
            .Select(d => new StatusDistribucijaDto
            {
                Status = d.Status,
                Broj = d.Count
            })
            .OrderByDescending(d => d.Broj)
            .ToList();
    }

    private async Task<List<Prihod30DanaDto>> GetPrihod30DanaAsync()
    {
        var prihod = await _dashboardRepository.GetPrihod30DanaAsync();
        
        // Popuniti prazne dane sa 0
        var startDate = DateTime.UtcNow.AddDays(-30).Date;
        var endDate = DateTime.UtcNow.Date;
        var sviDani = new List<Prihod30DanaDto>();

        for (var datum = startDate; datum <= endDate; datum = datum.AddDays(1))
        {
            var postojeci = prihod.FirstOrDefault(p => p.Datum.Date == datum.Date);
            sviDani.Add(new Prihod30DanaDto
            {
                Datum = datum,
                Suma = postojeci.Datum == default ? 0 : postojeci.Suma
            });
        }

        return sviDani;
    }

    private async Task<List<TopTuraDto>> GetTopTureAsync()
    {
        var ture = await _dashboardRepository.GetTopTureAsync(5);
        
        return ture.Select(t => new TopTuraDto
        {
            TuraId = t.TuraId,
            RedniBroj = t.RedniBroj,
            Relacija = t.Relacija,
            UlaznaCena = t.UlaznaCena ?? 0,
            PrevoznikNaziv = t.Prevoznik?.Naziv ?? "N/A",
            VoziloNaziv = t.Vozilo?.Naziv
        }).ToList();
    }

    private async Task<List<KriticnoVoziloDto>> GetKriticnaVozilaAsync()
    {
        var vozila = await _dashboardRepository.GetKriticnaVozilaAsync(7);
        var danas = DateTime.UtcNow;
        var thresholdDate = danas.AddDays(7);

        var kriticna = new List<KriticnoVoziloDto>();

        foreach (var vozilo in vozila)
        {
            // Registracija
            if (vozilo.RegistracijaDatumIsteka.HasValue &&
                vozilo.RegistracijaDatumIsteka.Value >= danas &&
                vozilo.RegistracijaDatumIsteka.Value <= thresholdDate)
            {
                kriticna.Add(new KriticnoVoziloDto
                {
                    VoziloId = vozilo.VoziloId,
                    Naziv = vozilo.Naziv,
                    TipProblema = "Registracija",
                    DatumIsteka = vozilo.RegistracijaDatumIsteka,
                    DanaDoIsteka = (int)(vozilo.RegistracijaDatumIsteka.Value - danas).TotalDays
                });
            }

            // Tehnički pregled
            if (vozilo.TehnickiPregledDatumIsteka.HasValue &&
                vozilo.TehnickiPregledDatumIsteka.Value >= danas &&
                vozilo.TehnickiPregledDatumIsteka.Value <= thresholdDate)
            {
                kriticna.Add(new KriticnoVoziloDto
                {
                    VoziloId = vozilo.VoziloId,
                    Naziv = vozilo.Naziv,
                    TipProblema = "Tehnički",
                    DatumIsteka = vozilo.TehnickiPregledDatumIsteka,
                    DanaDoIsteka = (int)(vozilo.TehnickiPregledDatumIsteka.Value - danas).TotalDays
                });
            }

            // PP Aparat
            if (vozilo.PPAparatDatumIsteka.HasValue &&
                vozilo.PPAparatDatumIsteka.Value >= danas &&
                vozilo.PPAparatDatumIsteka.Value <= thresholdDate)
            {
                kriticna.Add(new KriticnoVoziloDto
                {
                    VoziloId = vozilo.VoziloId,
                    Naziv = vozilo.Naziv,
                    TipProblema = "PP Aparat",
                    DatumIsteka = vozilo.PPAparatDatumIsteka,
                    DanaDoIsteka = (int)(vozilo.PPAparatDatumIsteka.Value - danas).TotalDays
                });
            }

            // Vinjete
            foreach (var vinjeta in vozilo.Vinjete.Where(v => 
                v.DatumIsteka >= danas && v.DatumIsteka <= thresholdDate))
            {
                kriticna.Add(new KriticnoVoziloDto
                {
                    VoziloId = vozilo.VoziloId,
                    Naziv = vozilo.Naziv,
                    TipProblema = $"Vinjeta ({vinjeta.DrzavaKod})",
                    DatumIsteka = vinjeta.DatumIsteka,
                    DanaDoIsteka = (int)(vinjeta.DatumIsteka - danas).TotalDays
                });
            }
        }

        return kriticna.OrderBy(k => k.DanaDoIsteka).ToList();
    }

    private async Task<List<LogDto>> GetLogoviAsync()
    {
        var logovi = await _dashboardRepository.GetNajnovijiLogoviAsync(10);
        
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

