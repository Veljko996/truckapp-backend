using WebApplication1.DataAccess.Models;
using WebApplication1.Repository.ReportRepository;
using WebApplication1.Utils.DTOs.NalogPrihodiDTO;
using WebApplication1.Utils.DTOs.ReportDTO;
using WebApplication1.Utils.Helper;

namespace WebApplication1.Services.ReportServices;

public class ReportService : IReportService
{
    private readonly IReportRepository _repo;

    public ReportService(IReportRepository repo)
    {
        _repo = repo;
    }

    public async Task<KrugoviReportDto> GetKrugoviReportAsync(DateTime from, DateTime to, int? voziloId)
    {
        var data = await _repo.GetKrugReportDataAsync(from, to, voziloId);

        var naloziByTura = data.Nalozi
            .GroupBy(n => n.TuraId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var gorivoByKrug = data.Gorivo
            .Where(g => g.KrugId.HasValue)
            .GroupBy(g => g.KrugId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var gorivoByNalog = data.Gorivo
            .Where(g => g.NalogId.HasValue)
            .GroupBy(g => g.NalogId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var rows = new List<KrugReportRowDto>();

        foreach (var krug in data.Krugovi)
        {
            var krugTureIds = krug.Ture.Select(t => t.TuraId).ToList();
            var krugNalozi = krugTureIds
                .SelectMany(tid => naloziByTura.TryGetValue(tid, out var l) ? l : Enumerable.Empty<Nalog>())
                .ToList();
            var krugNalogIds = krugNalozi.Select(n => n.NalogId).ToHashSet();

            // Gorivo kruga: KrugId == krug ILI NalogId u nalozima kruga (dedupe po GorivoZapisId).
            var gorivo = new Dictionary<int, GorivoZapis>();
            if (gorivoByKrug.TryGetValue(krug.KrugId, out var gk))
                foreach (var g in gk) gorivo[g.GorivoZapisId] = g;
            foreach (var nid in krugNalogIds)
                if (gorivoByNalog.TryGetValue(nid, out var gn))
                    foreach (var g in gn) gorivo[g.GorivoZapisId] = g;
            var gorivoList = gorivo.Values.ToList();

            var prihodi = FinancialCalc.BuildTotals(
                krugNalozi.SelectMany(n => n.Prihodi).Select(p => ((string?)p.Valuta, p.Iznos)));

            var troskovi = FinancialCalc.BuildTotals(
                krug.Troskovi.Select(t => ((string?)t.Valuta, t.Iznos))
                    .Concat(gorivoList.Select(g => ((string?)g.Valuta, g.Iznos)))
                    .Concat(krugNalozi.SelectMany(n => n.Troskovi).Select(t => ((string?)t.Valuta, t.Iznos))));

            var profit = FinancialCalc.Profit(prihodi, troskovi);

            int? predjeniKm = (krug.ZavrsnaKilometraza.HasValue && krug.PocetnaKilometraza.HasValue)
                ? krug.ZavrsnaKilometraza.Value - krug.PocetnaKilometraza.Value
                : null;

            rows.Add(new KrugReportRowDto
            {
                KrugId = krug.KrugId,
                Broj = krug.Broj,
                VoziloId = krug.VoziloId,
                VoziloNaziv = krug.Vozilo?.Naziv,
                Zatvoren = krug.ClosedAt,
                PredjeniKm = predjeniKm,
                Litara = gorivoList.Sum(g => g.KolicineLitara),
                BrojNaloga = krugNalozi.Count,
                Prihod = prihodi,
                Troskovi = troskovi,
                Profit = profit
            });
        }

        // Zbirno po vozilu (roll-up iz redova).
        var poVozilu = rows
            .GroupBy(r => r.VoziloId)
            .Select(g =>
            {
                var prihod = FinancialCalc.Merge(g.Select(r => r.Prihod));
                var troskovi = FinancialCalc.Merge(g.Select(r => r.Troskovi));
                return new VoziloReportRowDto
                {
                    VoziloId = g.Key,
                    VoziloNaziv = g.First().VoziloNaziv,
                    BrojKrugova = g.Count(),
                    PredjeniKm = g.Sum(r => r.PredjeniKm ?? 0),
                    Litara = g.Sum(r => r.Litara),
                    Prihod = prihod,
                    Troskovi = troskovi,
                    Profit = FinancialCalc.Profit(prihod, troskovi)
                };
            })
            .OrderByDescending(v => v.PredjeniKm)
            .ToList();

        // Ukupno (grand total).
        var ukupnoPrihod = FinancialCalc.Merge(rows.Select(r => r.Prihod));
        var ukupnoTroskovi = FinancialCalc.Merge(rows.Select(r => r.Troskovi));

        return new KrugoviReportDto
        {
            From = from,
            To = to,
            BrojKrugova = rows.Count,
            Krugovi = rows.OrderByDescending(r => r.Zatvoren).ToList(),
            PoVozilu = poVozilu,
            Ukupno = new ReportTotalsDto
            {
                Prihod = ukupnoPrihod,
                Troskovi = ukupnoTroskovi,
                Profit = FinancialCalc.Profit(ukupnoPrihod, ukupnoTroskovi)
            }
        };
    }
}
