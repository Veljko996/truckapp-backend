//using WebApplication1.Repository.DashboardRepository;
//using WebApplication1.Utils.DTOs.DashboardDTO;

//namespace WebApplication1.Services.DashboardServices;

//public class DashboardService : IDashboardService
//{
//    private readonly IDashboardRepository _dashboardRepository;
//    private readonly ILogger<DashboardService> _logger;

//    public DashboardService(IDashboardRepository dashboardRepository, ILogger<DashboardService> logger)
//    {
//        _dashboardRepository = dashboardRepository;
//        _logger = logger;
//    }

//    //public async Task<DashboardOverviewDto> GetDashboardOverviewAsync()
//    //{
//    //    try
//    //    {
//    //        var kpi = await GetKPIDataAsync();
//    //        //var statusDistribucija = await GetStatusDistribucijaAsync();
//    //        var prihod30Dana = await GetPrihod30DanaAsync();
//    //        var topTure = await GetTopTureAsync();
//    //        var kriticnaVozila = await GetKriticnaVozilaAsync();
//    //        var logovi = await GetLogoviAsync();

//    //        return new DashboardOverviewDto
//    //        //{
//    //        //    Kpi = kpi,
//    //        //    StatusDistribucija = statusDistribucija,
//    //            Prihod30Dana = prihod30Dana,
//    //            TopTure = topTure,
//    //            KriticnaVozila = kriticnaVozila,
//    //            Logovi = logovi
//    //        };
//    //    }
//    //    catch (Exception ex)
//    //    {
//    //        _logger.LogError(ex, "Greška pri dohvatanju dashboard podataka");
//    //        throw;
//    //    }
//    //}

//    //private async Task<KPIDto> GetKPIDataAsync()
//    //{
//    //    var allTure = await _dashboardRepository.GetAllTureWithIncludesAsync();
//    //    var allVozila = await _dashboardRepository.GetAllVozilaWithIncludesAsync();
//    //    var prihodByDate = await _dashboardRepository.GetPrihodByDateAsync();
        
//    //    var danas = DateTime.UtcNow.Date;

//    //    // Business logic: Define what "active" tours means
//    //    //var aktivniStatusi = new[]
//    //    //{
//    //    //    TuraStatus.UPripremi,
//    //    //    TuraStatus.NaPutu,
//    //    //    TuraStatus.UtovarUToku,
//    //    //    TuraStatus.IstovarUToku,
//    //    //    TuraStatus.Carina
//    //    //};
//    //    //var aktivneTure = allTure.Count(t => aktivniStatusi.Contains(t.StatusTure));

//    //    //// Business logic: Today's revenue (using DatumUtovaraOd for date comparison)
//    //    //var danasnjiPrihod = prihodByDate
//    //    //    .Where(p => p.Datum.Date == danas)
//    //    //    .Sum(p => p.Suma);

//    //    //// Business logic: Active vehicles = vehicles with tours that are not finished/cancelled
//    //    //var zavrseniStatusi = new[] { TuraStatus.Zavrseno, TuraStatus.Otkazano };
//    //    //var aktivnaVozila = allVozila
//    //    //    .Count(v => v.Ture.Any(t => !zavrseniStatusi.Contains(t.StatusTure)));

//    //    //// Business logic: Problematic tours = delayed or with problem status
//    //    //var problematickeTure = allTure.Count(t => 
//    //    //    t.StatusTure == TuraStatus.Zakasnjenje);

//    //    // Business logic: Vehicles with expiring documents (within 7 days)
//    //    var thresholdDate = DateTime.UtcNow.AddDays(7);
//    //    var today = DateTime.UtcNow;
//    //    var vozilaSaIsticucim = allVozila.Count(v =>
//    //        (v.RegistracijaDatumIsteka.HasValue &&
//    //         v.RegistracijaDatumIsteka.Value >= today &&
//    //         v.RegistracijaDatumIsteka.Value <= thresholdDate) ||
//    //        (v.TehnickiPregledDatumIsteka.HasValue &&
//    //         v.TehnickiPregledDatumIsteka.Value >= today &&
//    //         v.TehnickiPregledDatumIsteka.Value <= thresholdDate) ||
//    //        (v.PPAparatDatumIsteka.HasValue &&
//    //         v.PPAparatDatumIsteka.Value >= today &&
//    //         v.PPAparatDatumIsteka.Value <= thresholdDate) ||
//    //        v.Vinjete.Any(vin =>
//    //            vin.DatumIsteka >= today &&
//    //            vin.DatumIsteka <= thresholdDate));

//    //    //return new KPIDto
//    //    //{
//    //    //    AktivneTure = aktivneTure,
//    //    //    DanasnjiPrihod = danasnjiPrihod,
//    //    //    BrojAktivnihVozila = aktivnaVozila,
//    //    //    ProblematickeTure = problematickeTure,
//    //    //    VozilaSaIsticucimDokumentima = vozilaSaIsticucim
//    //    //};
//    //}

//    //private async Task<List<StatusDistribucijaDto>> GetStatusDistribucijaAsync()
//    //{
//    //    var distribucija = await _dashboardRepository.GetStatusDistribucijaAsync();
        
//    //    // Filtrirati samo relevantne statuse za prikaz
//    //    var relevantniStatusi = new[] 
//    //    { 
//    //        TuraStatus.UPripremi, 
//    //        TuraStatus.NaPutu, 
//    //        TuraStatus.Zavrseno, 
//    //        TuraStatus.Zakasnjenje 
//    //    };

//    //    return distribucija
//    //        .Where(d => relevantniStatusi.Contains(d.Status))
//    //        .Select(d => new StatusDistribucijaDto
//    //        {
//    //            Status = d.Status,
//    //            Broj = d.Count
//    //        })
//    //        .OrderByDescending(d => d.Broj)
//    //        .ToList();
//    //}

//    private async Task<List<Prihod30DanaDto>> GetPrihod30DanaAsync()
//    {
//        var prihodByDate = await _dashboardRepository.GetPrihodByDateAsync();
        
//        // Business logic: Filter for last 30 days
//        var startDate = DateTime.UtcNow.AddDays(-30).Date;
//        var endDate = DateTime.UtcNow.Date;
        
//        var prihod30Dana = prihodByDate
//            .Where(p => p.Datum >= startDate && p.Datum <= endDate)
//            .ToList();

//        // Fill missing days with 0
//        var sviDani = new List<Prihod30DanaDto>();
//        for (var datum = startDate; datum <= endDate; datum = datum.AddDays(1))
//        {
//            var postojeci = prihod30Dana.FirstOrDefault(p => p.Datum.Date == datum.Date);
//            sviDani.Add(new Prihod30DanaDto
//            {
//                Datum = datum,
//                Suma = postojeci.Datum == default ? 0 : postojeci.Suma
//            });
//        }

//        return sviDani;
//    }

//    private async Task<List<TopTuraDto>> GetTopTureAsync()
//    {
//        var allTure = await _dashboardRepository.GetAllTureWithIncludesAsync();
        
//        // Business logic: Top tours = tours with price > 0, ordered by price descending
//        var topTure = allTure
//            .Where(t => t.UlaznaCena.HasValue && t.UlaznaCena > 0)
//            .OrderByDescending(t => t.UlaznaCena)
//            .Take(5)
//            .ToList();
        
//        return topTure.Select(t => new TopTuraDto
//        {
//            TuraId = t.TuraId,
//            RedniBroj = t.RedniBroj,
//            Relacija = t.MestoIstovara + " - " + t.MestoUtovara,
//            UlaznaCena = t.UlaznaCena ?? 0,
//            PrevoznikNaziv = t.Prevoznik?.Naziv ?? "N/A",
//            VoziloNaziv = t.Vozilo?.Naziv
//        }).ToList();
//    }

//    private async Task<List<KriticnoVoziloDto>> GetKriticnaVozilaAsync()
//    {
//        var allVozila = await _dashboardRepository.GetAllVozilaWithIncludesAsync();
//        var danas = DateTime.UtcNow;
//        var thresholdDate = danas.AddDays(7);

//        var kriticna = new List<KriticnoVoziloDto>();

//        // Business logic: Critical vehicles = vehicles with documents expiring within 7 days
//        foreach (var vozilo in allVozila)
//        {
//            // Registracija
//            if (vozilo.RegistracijaDatumIsteka.HasValue &&
//                vozilo.RegistracijaDatumIsteka.Value >= danas &&
//                vozilo.RegistracijaDatumIsteka.Value <= thresholdDate)
//            {
//                kriticna.Add(new KriticnoVoziloDto
//                {
//                    VoziloId = vozilo.VoziloId,
//                    Naziv = vozilo.Naziv,
//                    TipProblema = "Registracija",
//                    DatumIsteka = vozilo.RegistracijaDatumIsteka,
//                    DanaDoIsteka = (int)(vozilo.RegistracijaDatumIsteka.Value - danas).TotalDays
//                });
//            }

//            // Tehnički pregled
//            if (vozilo.TehnickiPregledDatumIsteka.HasValue &&
//                vozilo.TehnickiPregledDatumIsteka.Value >= danas &&
//                vozilo.TehnickiPregledDatumIsteka.Value <= thresholdDate)
//            {
//                kriticna.Add(new KriticnoVoziloDto
//                {
//                    VoziloId = vozilo.VoziloId,
//                    Naziv = vozilo.Naziv,
//                    TipProblema = "Tehnički",
//                    DatumIsteka = vozilo.TehnickiPregledDatumIsteka,
//                    DanaDoIsteka = (int)(vozilo.TehnickiPregledDatumIsteka.Value - danas).TotalDays
//                });
//            }

//            // PP Aparat
//            if (vozilo.PPAparatDatumIsteka.HasValue &&
//                vozilo.PPAparatDatumIsteka.Value >= danas &&
//                vozilo.PPAparatDatumIsteka.Value <= thresholdDate)
//            {
//                kriticna.Add(new KriticnoVoziloDto
//                {
//                    VoziloId = vozilo.VoziloId,
//                    Naziv = vozilo.Naziv,
//                    TipProblema = "PP Aparat",
//                    DatumIsteka = vozilo.PPAparatDatumIsteka,
//                    DanaDoIsteka = (int)(vozilo.PPAparatDatumIsteka.Value - danas).TotalDays
//                });
//            }

//            // Vinjete
//            foreach (var vinjeta in vozilo.Vinjete.Where(v => 
//                v.DatumIsteka >= danas && v.DatumIsteka <= thresholdDate))
//            {
//                kriticna.Add(new KriticnoVoziloDto
//                {
//                    VoziloId = vozilo.VoziloId,
//                    Naziv = vozilo.Naziv,
//                    TipProblema = $"Vinjeta ({vinjeta.DrzavaKod})",
//                    DatumIsteka = vinjeta.DatumIsteka,
//                    DanaDoIsteka = (int)(vinjeta.DatumIsteka - danas).TotalDays
//                });
//            }
//        }

//        return kriticna.OrderBy(k => k.DanaDoIsteka).ToList();
//    }

//    private async Task<List<LogDto>> GetLogoviAsync()
//    {
//        var logovi = await _dashboardRepository.GetNajnovijiLogoviAsync(10);
        
//        return logovi.Select(l => new LogDto
//        {
//            LogId = l.LogId,
//            HappenedAtDate = l.HappenedAtDate,
//            Process = l.Process,
//            Activity = l.Activity,
//            Message = l.Message,
//            UserName = l.User?.Username
//        }).ToList();
//    }
//}

