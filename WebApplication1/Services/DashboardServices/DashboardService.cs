using WebApplication1.Repository.DashboardRepository;
using WebApplication1.Utils.DTOs.DashboardDTO;

namespace WebApplication1.Services.DashboardServices;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _repository;

    public DashboardService(IDashboardRepository repository)
    {
        _repository = repository;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetDashboardStatsAsync(cancellationToken);
    }

    public async Task<List<DashboardMonthlyProfitDto>> GetMonthlyProfitAsync(int monthsBack = 12, CancellationToken cancellationToken = default)
    {
        return await _repository.GetMonthlyProfitAsync(monthsBack, cancellationToken);
    }

    public async Task<List<KriticnoVoziloDto>> GetKriticnaVozilaAsync(int daysThreshold = 7, CancellationToken cancellationToken = default)
    {
        var vozila = await _repository.GetVozilaSaIsticucimDokumentimaAsync(daysThreshold, cancellationToken);
        var danas = DateTime.UtcNow.Date;
        var thresholdDate = danas.AddDays(daysThreshold);
        var kriticna = new List<KriticnoVoziloDto>();

        foreach (var vozilo in vozila)
        {
            if (vozilo.RegistracijaDatumIsteka.HasValue && vozilo.RegistracijaDatumIsteka.Value.Date <= thresholdDate)
                kriticna.Add(new KriticnoVoziloDto { VoziloId = vozilo.VoziloId, Naziv = vozilo.Naziv, TipProblema = "Registracija", DatumIsteka = vozilo.RegistracijaDatumIsteka, DanaDoIsteka = (int)(vozilo.RegistracijaDatumIsteka.Value.Date - danas).TotalDays });

            if (vozilo.TehnickiPregledDatumIsteka.HasValue && vozilo.TehnickiPregledDatumIsteka.Value.Date <= thresholdDate)
                kriticna.Add(new KriticnoVoziloDto { VoziloId = vozilo.VoziloId, Naziv = vozilo.Naziv, TipProblema = "Tehnički pregled", DatumIsteka = vozilo.TehnickiPregledDatumIsteka, DanaDoIsteka = (int)(vozilo.TehnickiPregledDatumIsteka.Value.Date - danas).TotalDays });

            if (vozilo.PPAparatDatumIsteka.HasValue && vozilo.PPAparatDatumIsteka.Value.Date <= thresholdDate)
                kriticna.Add(new KriticnoVoziloDto { VoziloId = vozilo.VoziloId, Naziv = vozilo.Naziv, TipProblema = "PP Aparat", DatumIsteka = vozilo.PPAparatDatumIsteka, DanaDoIsteka = (int)(vozilo.PPAparatDatumIsteka.Value.Date - danas).TotalDays });

            foreach (var vinjeta in vozilo.Vinjete.Where(v => v.DatumIsteka.Date <= thresholdDate))
                kriticna.Add(new KriticnoVoziloDto { VoziloId = vozilo.VoziloId, Naziv = vozilo.Naziv, TipProblema = "Vinjeta", DatumIsteka = vinjeta.DatumIsteka, DanaDoIsteka = (int)(vinjeta.DatumIsteka.Date - danas).TotalDays, Detalji = $"{vinjeta.DrzavaKod} ({vinjeta.Drzava ?? ""})" });
        }

        return kriticna.OrderBy(k => k.DanaDoIsteka).ThenBy(k => k.VoziloId).ToList();
    }
}
