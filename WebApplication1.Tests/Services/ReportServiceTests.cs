using Moq;
using System.Linq;
using System.Collections.Generic;
using WebApplication1.DataAccess.Models;
using WebApplication1.Repository.ReportRepository;
using WebApplication1.Services.ReportServices;
using WebApplication1.Utils.DTOs.NalogPrihodiDTO;

namespace WebApplication1.Tests.Services;

[TestFixture]
public class ReportServiceTests
{
    private Mock<IReportRepository> _repoMock;
    private ReportService _service;

    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<IReportRepository>();
        _service = new ReportService(_repoMock.Object);
    }

    private static decimal Amt(IEnumerable<AmountByCurrencyDto> list, string cur)
        => list.FirstOrDefault(x => x.Currency == cur)?.Amount ?? 0m;

    [Test]
    public async Task GetKrugoviReportAsync_ComputesPerKrug_PerVozilo_AndGrandTotal_PerCurrency()
    {
        // --- Dataset ---
        // Krug A (vozilo 1): Nalog N1 -> Prihod 1000 EUR, Trosak 100 EUR; KrugTrosak 50 EUR; Gorivo 200 EUR (po KrugId)
        //   troskovi = 50+200+100 = 350 EUR; prihod 1000 EUR; profit 650 EUR
        // Krug B (vozilo 1): Nalog N2 -> Prihod 500 RSD; Gorivo 100 RSD (po NalogId)
        //   troskovi 100 RSD; prihod 500 RSD; profit 400 RSD
        // Krug C (vozilo 2): Nalog N3 -> Prihod 2000 EUR; bez troskova -> profit 2000 EUR
        var vozilo1 = new NasaVozila { VoziloId = 1, Naziv = "Scania" };
        var vozilo2 = new NasaVozila { VoziloId = 2, Naziv = "Volvo" };

        var krugA = new Krug
        {
            KrugId = 10, VoziloId = 1, Vozilo = vozilo1, Status = "Zatvoren",
            PocetnaKilometraza = 1000, ZavrsnaKilometraza = 1500,
            Ture = new List<Tura> { new Tura { TuraId = 100 } },
            Troskovi = new List<KrugTrosak> { new KrugTrosak { KrugTrosakId = 1, KrugId = 10, Iznos = 50m, Valuta = "EUR" } }
        };
        var krugB = new Krug
        {
            KrugId = 11, VoziloId = 1, Vozilo = vozilo1, Status = "Zatvoren",
            PocetnaKilometraza = 2000, ZavrsnaKilometraza = 2300,
            Ture = new List<Tura> { new Tura { TuraId = 101 } },
            Troskovi = new List<KrugTrosak>()
        };
        var krugC = new Krug
        {
            KrugId = 12, VoziloId = 2, Vozilo = vozilo2, Status = "Zatvoren",
            Ture = new List<Tura> { new Tura { TuraId = 102 } },
            Troskovi = new List<KrugTrosak>()
        };

        var n1 = new Nalog
        {
            NalogId = 200, TuraId = 100, StatusNaloga = "Zavrsen",
            Prihodi = new List<NalogPrihod> { new NalogPrihod { Iznos = 1000m, Valuta = "EUR" } },
            Troskovi = new List<NalogTrosak> { new NalogTrosak { Iznos = 100m, Valuta = "EUR" } }
        };
        var n2 = new Nalog
        {
            NalogId = 201, TuraId = 101, StatusNaloga = "Zavrsen",
            Prihodi = new List<NalogPrihod> { new NalogPrihod { Iznos = 500m, Valuta = "RSD" } },
            Troskovi = new List<NalogTrosak>()
        };
        var n3 = new Nalog
        {
            NalogId = 202, TuraId = 102, StatusNaloga = "Zavrsen",
            Prihodi = new List<NalogPrihod> { new NalogPrihod { Iznos = 2000m, Valuta = "EUR" } },
            Troskovi = new List<NalogTrosak>()
        };

        var gorivo = new List<GorivoZapis>
        {
            new GorivoZapis { GorivoZapisId = 1, KrugId = 10, Iznos = 200m, Valuta = "EUR", KolicineLitara = 150m },
            new GorivoZapis { GorivoZapisId = 2, NalogId = 201, Iznos = 100m, Valuta = "RSD", KolicineLitara = 60m },
        };

        _repoMock
            .Setup(r => r.GetKrugReportDataAsync(It.IsAny<System.DateTime>(), It.IsAny<System.DateTime>(), It.IsAny<int?>()))
            .ReturnsAsync(new KrugReportData
            {
                Krugovi = new List<Krug> { krugA, krugB, krugC },
                Nalozi = new List<Nalog> { n1, n2, n3 },
                Gorivo = gorivo
            });

        // --- Act ---
        var report = await _service.GetKrugoviReportAsync(System.DateTime.UtcNow.AddDays(-30), System.DateTime.UtcNow, null);

        // --- Assert: po krugu ---
        Assert.That(report.BrojKrugova, Is.EqualTo(3));
        var rA = report.Krugovi.First(k => k.KrugId == 10);
        var rB = report.Krugovi.First(k => k.KrugId == 11);
        var rC = report.Krugovi.First(k => k.KrugId == 12);
        Assert.Multiple(() =>
        {
            Assert.That(Amt(rA.Prihod, "EUR"), Is.EqualTo(1000m));
            Assert.That(Amt(rA.Troskovi, "EUR"), Is.EqualTo(350m), "50 krug + 200 gorivo + 100 nalog");
            Assert.That(Amt(rA.Profit, "EUR"), Is.EqualTo(650m));
            Assert.That(rA.PredjeniKm, Is.EqualTo(500));
            Assert.That(rA.Litara, Is.EqualTo(150m));

            Assert.That(Amt(rB.Prihod, "RSD"), Is.EqualTo(500m));
            Assert.That(Amt(rB.Troskovi, "RSD"), Is.EqualTo(100m), "gorivo po NalogId");
            Assert.That(Amt(rB.Profit, "RSD"), Is.EqualTo(400m));

            Assert.That(Amt(rC.Profit, "EUR"), Is.EqualTo(2000m));
        });

        // --- Assert: po vozilu ---
        var v1 = report.PoVozilu.First(v => v.VoziloId == 1);
        var v2 = report.PoVozilu.First(v => v.VoziloId == 2);
        Assert.Multiple(() =>
        {
            Assert.That(v1.BrojKrugova, Is.EqualTo(2));
            Assert.That(v1.PredjeniKm, Is.EqualTo(800), "500 + 300");
            Assert.That(Amt(v1.Profit, "EUR"), Is.EqualTo(650m));
            Assert.That(Amt(v1.Profit, "RSD"), Is.EqualTo(400m));
            Assert.That(Amt(v2.Profit, "EUR"), Is.EqualTo(2000m));
        });

        // --- Assert: ukupno ---
        Assert.Multiple(() =>
        {
            Assert.That(Amt(report.Ukupno.Prihod, "EUR"), Is.EqualTo(3000m));
            Assert.That(Amt(report.Ukupno.Prihod, "RSD"), Is.EqualTo(500m));
            Assert.That(Amt(report.Ukupno.Troskovi, "EUR"), Is.EqualTo(350m));
            Assert.That(Amt(report.Ukupno.Profit, "EUR"), Is.EqualTo(2650m));
            Assert.That(Amt(report.Ukupno.Profit, "RSD"), Is.EqualTo(400m));
        });
    }

    [Test]
    public async Task GetKrugoviReportAsync_WhenNoKrugovi_ReturnsEmpty()
    {
        _repoMock
            .Setup(r => r.GetKrugReportDataAsync(It.IsAny<System.DateTime>(), It.IsAny<System.DateTime>(), It.IsAny<int?>()))
            .ReturnsAsync(new KrugReportData());

        var report = await _service.GetKrugoviReportAsync(System.DateTime.UtcNow.AddDays(-1), System.DateTime.UtcNow, null);

        Assert.That(report.BrojKrugova, Is.EqualTo(0));
        Assert.That(report.Krugovi, Is.Empty);
        Assert.That(report.PoVozilu, Is.Empty);
        Assert.That(report.Ukupno.Profit, Is.Empty);
    }
}
