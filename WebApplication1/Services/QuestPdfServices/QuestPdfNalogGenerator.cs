using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WebApplication1.Repository.NalogRepository;

namespace WebApplication1.Services.QuestPdfServices;

public class QuestPdfNalogGenerator : IQuestPdfNalogGenerator
{
    private readonly INalogRepository _nalogRepository;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<QuestPdfNalogGenerator> _logger;
    private static byte[]? _logoCache;
    private static readonly object _logoLock = new();

    public QuestPdfNalogGenerator(
        INalogRepository nalogRepository,
        IWebHostEnvironment env,
        ILogger<QuestPdfNalogGenerator> logger)
    {
        _nalogRepository = nalogRepository;
        _env = env;
        _logger = logger;
    }

    public async Task<byte[]> GeneratePdfAsync(int nalogId, string templateKey)
    {
        var nalog = await _nalogRepository.GetByIdAsync(nalogId)
            ?? throw new NotFoundException("Nalog", nalogId);

        var logoBytes = await LoadLogoAsync();

        var document = templateKey.ToLowerInvariant() switch
        {
            "mts"      => BuildMtsDocument(nalog, logoBytes),
            "suins"    => BuildSuinsDocument(nalog, logoBytes),
            "timnalog" => BuildTallTeamDocument(nalog, logoBytes),
        };

        return document.GeneratePdf();
    }

    private async Task<byte[]?> LoadLogoAsync()
    {
        if (_logoCache != null)
            return _logoCache;

        lock (_logoLock)
        {
            if (_logoCache != null)
                return _logoCache;

            try
            {
                var logoPath = Path.Combine(_env.ContentRootPath, "Resources", "logo.png");
                if (File.Exists(logoPath))
                {
                    _logoCache = File.ReadAllBytes(logoPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Nije moguće učitati logo za PDF generisanje");
                _logoCache = null;
            }
        }

        return _logoCache;
    }

    private static IDocument BuildMtsDocument(Nalog nalog, byte[]? logoBytes)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                // Jednostavan margin bez korišćenja Unit enum-a (API se menjao između verzija)
                page.Margin(40);
                page.DefaultTextStyle(TextStyle.Default.FontFamily(Fonts.TimesRoman).FontSize(14));

                page.Content()
                    .Column(column =>
                    {
                        // HEADER
                        column.Item().Row(row =>
                        {
                            row.RelativeItem(3).Column(col =>
                            {
                                if (logoBytes != null)
                                {
                                    // Širina se podešava na kontejneru, ne na ImageDescriptor-u
                                    col.Item().Width(120).Image(logoBytes).FitWidth();
                                }
                            });
                            row.RelativeItem(7).PaddingLeft(10).Column(col =>
                            {
                                col.Item().Text("MTS doo").Bold();
                                col.Item().Text("Branka Bajića 12, 21000 Novi Sad");
                                col.Item().Text("MB: 21884995 ; PIB: 113536304");
                            });
                        });

                        column.Item().PaddingTop(4).AlignCenter().Text("www.suins.rs").FontSize(12);
                        column.Item().LineHorizontal(2).LineColor(Colors.Black);
                        column.Item().PaddingVertical(8);

                        // NASLOV
                        column.Item().AlignCenter().Text($"Nalog za transport: {nalog.NalogBroj ?? ""}").Bold().FontSize(16);

                        column.Item().PaddingTop(10);

                        // DAVALAC / PRIMALAC
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Davalac naloga:").Bold();
                                col.Item().Text("Firma: SUINS MTS DOO");
                                col.Item().Text("Ime: Stefan Miletić");
                                col.Item().Text("Tel.: +381 63 512 425");
                                col.Item().Text("E-mail: transport@suins.rs");
                            });
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Primalac naloga:").Bold();
                                // Background se primenjuje na kontejner, ne na TextBlockDescriptor
                                col.Item().Background(Colors.Grey.Lighten3).Text($"Firma: {nalog.Prevoznik?.Naziv ?? ""}");
                                col.Item().Background(Colors.Grey.Lighten3).Text($"Ime: {nalog.Prevoznik?.Kontakt ?? ""}");
                                col.Item().Background(Colors.Grey.Lighten3).Text($"Tel: {nalog.Prevoznik?.Telefon ?? ""}");
                                col.Item().Background(Colors.Grey.Lighten3).Text("E-mail:");
                            });
                        });

                        column.Item().PaddingTop(10);

                        // PODACI NALOGA
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Podaci naloga").Bold();
                            row.RelativeItem().AlignCenter().Text("Predmet: Transport").Bold();
                            row.RelativeItem().AlignRight().Text($"Datum: {nalog.CreatedAt:dd.MM.yyyy}").Bold();
                        });

                        column.Item().PaddingTop(5);

                        // TABELA SA PODACIMA
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(3);
                            });

                            AddTableRow(table, "Broj vozila:", nalog.Tura?.Vozilo?.Naziv ?? "");
                            AddTableRow(table, "Registarski broj vozila:", nalog.RegistarskiBrojVozilaExt ?? "");
                            AddTableRow(table, "Relacija:", nalog.Relacija ?? "");
                            AddTableRow(table, "Datum i vreme utovara:", 
                                nalog.DatumUtovara?.ToString("dd.MM.yyyy HH:mm") ?? "");
                            AddTableRow(table, "Mesto utovara:", nalog.Tura?.MestoUtovara ?? "");
                            AddTableRow(table, "Adresa utovara:", nalog.AdresaUtovara ?? "");
                            AddTableRow(table, "Količina robe:", 
                                FormatKolicinaITezina(nalog.KolicinaRobe, nalog.Tura?.Tezina));
                            AddTableRow(table, "Vrsta robe:", nalog.VrstaRobe ?? "");
                            AddTableRow(table, "Izvoznik:", nalog.Izvoznik ?? "");
                            AddTableRow(table, "Izvozno carinjenje:", nalog.IzvoznoCarinjenje ?? "");
                            AddTableRow(table, "Granični prelaz ulaska u Srbiju:", nalog.GranicniPrelaz ?? "");
                            AddTableRow(table, "Uvoznik:", nalog.Uvoznik ?? "");
                            AddTableRow(table, "Uvozno carinjenje:", nalog.UvoznoCarinjenje ?? "");
                            AddTableRow(table, "Špedicija:", nalog.Spedicija ?? "");
                            AddTableRow(table, "Datum istovara:", 
                                nalog.DatumIstovara?.ToString("dd.MM.yyyy HH:mm") ?? "");
                            AddTableRow(table, "Mesto istovara:", nalog.Tura?.MestoIstovara ?? "");
                            AddTableRow(table, "Adresa istovara:", nalog.AdresaIstovara ?? "");
                        });

                        column.Item().PaddingTop(10);

                        // NAPOMENA
                        if (!string.IsNullOrWhiteSpace(nalog.NapomenaNalog))
                        {
                            column.Item().Text("Napomena:").Bold();
                            column.Item().Text(nalog.NapomenaNalog ?? "");
                        }

                        column.Item().PaddingTop(10);
                    });
            });

            // DRUGA STRANA
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(TextStyle.Default.FontFamily(Fonts.TimesRoman).FontSize(11));

                page.Content()
                    .Column(column =>
                    {
                        // HEADER (isti kao na prvoj strani)
                        column.Item().Row(row =>
                        {
                            row.RelativeItem(3).Column(col =>
                            {
                                if (logoBytes != null)
                                {
                                    col.Item().Width(120).Image(logoBytes).FitWidth();
                                }
                            });
                            row.RelativeItem(7).PaddingLeft(10).Column(col =>
                            {
                                col.Item().Text("MTS doo").Bold().FontSize(14);
                                col.Item().Text("Branka Bajića 12, 21000 Novi Sad");
                                col.Item().Text("MB: 21884995 ; PIB: 113536304");
                            });
                        });

                        column.Item().PaddingTop(4).AlignCenter().Text("www.suins.rs").FontSize(12);
                        column.Item().LineHorizontal(2).LineColor(Colors.Black);
                        column.Item().PaddingVertical(5);

                        column.Item().AlignCenter().Text($"Nalog za transport: {nalog.NalogBroj ?? ""}").Bold().FontSize(14);

                        column.Item().PaddingTop(5);

                        // FINANSIJSKI PODACI
                        // Format price with currency - append "+ Terminali + PDV" only for RSD
                        var cena = nalog.Tura?.UlaznaCena?.ToString("N2") ?? "";
                        var valuta = (nalog.Tura?.Valuta ?? "").Trim();
                        var isRsd = valuta.ToUpper() == "RSD";
                        var cenaTransporta = string.IsNullOrWhiteSpace(cena) 
                            ? valuta 
                            : $"{cena} {valuta}" + (isRsd ? " + Terminali + PDV" : "");

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(7);
                            });

                            AddTableRowSmall(table, "Cena transporta:", cenaTransporta);
                            AddTableRowSmall(table, "Valuta plaćanja:", 
                                "60 dana od dana prijema originalne overene dokumentacije (CMR,EX)");
                            AddTableRowSmall(table, "Način plaćanja:", 
                                "Plaćanje po srednjem kursu NBS-a");
                            AddTableRowSmall(table, "Napomena:", 
                                "Faktura će preko SEF biti prihvaćena samo ukoliko sadrži dokaz o izvršenoj usluzi i svu neophodnu dokumentaciju. " +
                                "Obavezno je upisivanje broja naloga u polje Order Reference.\n\n" +
                                "Rok za dostavu dokumentacije je 15 dana od datuma istovara. U slučaju kašnjenja sa istom, faktura će Vam biti umanjena za 1%.");
                        });

                        column.Item().PaddingTop(10);

                        // USLOVI
                        column.Item().Text("Primalac naloga prihvata sledeće uslove:").Bold();
                        column.Item().PaddingTop(3).Text("1. Zaštita imena poslovnih partnera mora biti zagarantovana, direktan kontakt sa našim klijentom bez našeg znanja je najstrožije zabranjen;");
                        column.Item().Text("2. Pretovar, dotovar bez našeg znanja nije dozvoljen;");
                        column.Item().Text("3. Eventualna nezgoda, kvar ili bilo kakvo drugo odugovlačenje sa datumom utovara/istovara mora se javiti u pismenoj formi. Vozilo mora imati CMR osiguranje, u suprotnom svi naknadni troškovi ili nezgode idu na Vas. Ukoliko nama odbiju procenat od vozarine prenećemo taj trošak na Vas. CMR osiguranje nam se mora dostaviti pre utovara. U suprotnom se ne priznaje;");
                        column.Item().Text("4. Plaćanje transporta nije moguće bez originala, pečatiranog i potpisanog CMR-a bez ikakvih primedbi od strane primaoca. U slučaju bilo kakvih problema odmah nas kontaktirajte;");
                        column.Item().Text("5. Kamioni imaju 24 sata za utovar i 48 sati za istovar besplatno. Subota posle 16:00 h i nedelja se ne računaju u pomenuto vreme;");
                        column.Item().Text("6. Vozač mora prisustvovati utovaru/istovaru i robu primiti/predati uredno po broju koleata prema dokumentima koji moraju biti uredno potpisani i overeni;");
                        column.Item().Text("7. Vozač je odgovoran za osovinsko opterećenje vozila i dužan je da vodi računa o pravilnom rasporedu tereta. Za ukupno opterećenje vozila vozač ima pravo na nadoknadu uz originalnu priznanicu;");
                        column.Item().Text("8. Prilikom utovara španeri su obavezni; Vozila na utovar moraju doći ispravna i čista sa ispravnom ceradom;");
                        column.Item().Text("9. Bilo kakvi naknadni dogovori sa isporučiocem/primaocem robe, a mimo našeg naloga i znanja su zabranjeni;");
                        column.Item().Text("10. Bilo kakav nedostatak ili uzorkovanje robe od strane carinika ili špeditera mora biti upisano u CMR, u suprotnom će vozač biti zadužen za svaki nedostatak robe;");
                        column.Item().Text("11. Ako vozač dobije T5 ili TC11 dokument, obavezno ga mora overiti na zadnjoj Evropskoj granici i dostaviti ga nama u originalu; overen EX-1/T1 dokument obavezno dostaviti nama jer je to uslov naplate vozarine;");
                        column.Item().Text("12. U slučaju da bilo koji od navedenih uslova nije zadovoljen, ne možemo Vam garantovati isplatu vozarine ili nekog dela iste.");

                        column.Item().PaddingTop(10);
                        column.Item().Text("U slučaju spora nadležan je sud u Novom Sadu");
                        column.Item().Text("Ovu stranu naloga ne smete pokazivati na utovaru/istovaru!!!");
                        column.Item().Text($"Uslov naplate vozarine    Datum: {nalog.CreatedAt:dd.MM.yyyy}");

                        column.Item().PaddingTop(10);
                        column.Item().LineHorizontal(1).LineColor(Colors.Black);
                        column.Item().PaddingTop(5).AlignCenter().Text("www.suins.rs").FontSize(12);
                    });
            });
        });
    }

    // ========== SUINS LAYOUT (zasnovan na SuinsNalogTemplate) ==========
    private static IDocument BuildSuinsDocument(Nalog nalog, byte[]? logoBytes)
    {
        // SUINS layout je identičan MTS layout-u, osim header-a gde ima samo logo (bez teksta)
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(TextStyle.Default.FontFamily(Fonts.TimesRoman).FontSize(14));

                page.Content()
                    .Column(column =>
                    {
                        // HEADER
                        column.Item().Row(row =>
                        {
                            row.RelativeItem(3).Column(col =>
                            {
                                if (logoBytes != null)
                                {
                                    col.Item().Width(120).Image(logoBytes).FitWidth();
                                }
                            });
                            row.RelativeItem(7).PaddingLeft(10).Column(col =>
                            {
                                col.Item().Text("SUINS DOO NOVI SAD, BRANKA BAJIĆA 12, 21000 Novi Sad").Bold().FontSize(10);
                                col.Item().Text("PIB: 107024756; Matični broj: 20726695; šifra delatnosti: 5229").FontSize(9);
                                col.Item().Text("Sedište: Novi Sad Branka Bajića 12; tel: 021/6393-416; office@suins.rs").FontSize(9);
                                col.Item().Text("PJ Subotica: +381 (0)24 551-651; fax: +381 (0)24 551-113; e-mail: subotica@suins.rs").FontSize(9);
                                col.Item().Text("PJ Horgoš: +381 69 795 014; horgoš@suins.rs").FontSize(9);
                                col.Item().Text("PJ Kelebija: +381 (0)24 789 071").FontSize(9);
                            });
                        });

                        column.Item().PaddingTop(4).AlignCenter().Text("www.suins.rs").FontSize(12);
                        column.Item().LineHorizontal(2).LineColor(Colors.Black);
                        column.Item().PaddingVertical(8);

                        // NASLOV
                        column.Item().AlignCenter().Text($"Nalog za transport: {nalog.NalogBroj ?? ""}").Bold().FontSize(16);

                        column.Item().PaddingTop(10);

                        // DAVALAC / PRIMALAC
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Davalac naloga:").Bold();
                                col.Item().Text("Firma: SUINS DOO Novi Sad");
                                col.Item().Text("Ime: Stefan Miletić");
                                col.Item().Text("Tel: +381 63 512 425");
                                col.Item().Text("E-mail: transport@suins.rs");
                            });
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Primalac naloga:").Bold();
                                col.Item().Background(Colors.Grey.Lighten3).Text($"Firma: {nalog.Prevoznik?.Naziv ?? ""}");
                                col.Item().Background(Colors.Grey.Lighten3).Text($"Ime: {nalog.Prevoznik?.Kontakt ?? ""}");
                                col.Item().Background(Colors.Grey.Lighten3).Text($"Tel: {nalog.Prevoznik?.Telefon ?? ""}");
                                col.Item().Background(Colors.Grey.Lighten3).Text("E-mail:");
                            });
                        });

                        column.Item().PaddingTop(10);

                        // PODACI NALOGA
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Podaci naloga").Bold();
                            row.RelativeItem().AlignCenter().Text("Predmet: Transport").Bold();
                            row.RelativeItem().AlignRight().Text($"Datum: {nalog.CreatedAt:dd.MM.yyyy}").Bold();
                        });

                        column.Item().PaddingTop(5);

                        // TABELA SA PODACIMA
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(3);
                            });

                            AddTableRow(table, "Broj vozila:", nalog.Tura?.Vozilo?.Naziv ?? "");
                            AddTableRow(table, "Registarski broj vozila:", nalog.RegistarskiBrojVozilaExt ?? "");
                            AddTableRow(table, "Relacija:", nalog.Relacija ?? "");
                            AddTableRow(table, "Datum i vreme utovara:", 
                                nalog.DatumUtovara?.ToString("dd.MM.yyyy HH:mm") ?? "");
                            AddTableRow(table, "Mesto utovara:", nalog.Tura?.MestoUtovara ?? "");
                            AddTableRow(table, "Adresa utovara:", nalog.AdresaUtovara ?? "");
                            AddTableRow(table, "Količina robe:", 
                                FormatKolicinaITezina(nalog.KolicinaRobe, nalog.Tura?.Tezina));
                            AddTableRow(table, "Vrsta robe:", nalog.VrstaRobe ?? "");
                            AddTableRow(table, "Izvoznik:", nalog.Izvoznik ?? "");
                            AddTableRow(table, "Izvozno carinjenje:", nalog.IzvoznoCarinjenje ?? "");
                            AddTableRow(table, "Granični prelaz ulaska u Srbiju:", nalog.GranicniPrelaz ?? "");
                            AddTableRow(table, "Uvoznik:", nalog.Uvoznik ?? "");
                            AddTableRow(table, "Uvozno carinjenje:", nalog.UvoznoCarinjenje ?? "");
                            AddTableRow(table, "Špedicija:", nalog.Spedicija ?? "");
                            AddTableRow(table, "Datum istovara:", 
                                nalog.DatumIstovara?.ToString("dd.MM.yyyy HH:mm") ?? "");
                            AddTableRow(table, "Mesto istovara:", nalog.Tura?.MestoIstovara ?? "");
                            AddTableRow(table, "Adresa istovara:", nalog.AdresaIstovara ?? "");
                        });

                        column.Item().PaddingTop(10);

                        // NAPOMENA
                        if (!string.IsNullOrWhiteSpace(nalog.NapomenaNalog))
                        {
                            column.Item().Text("Napomena:").Bold();
                            column.Item().Text(nalog.NapomenaNalog ?? "");
                        }

                        column.Item().PaddingTop(10);
                    });
            });

            // DRUGA STRANA
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(TextStyle.Default.FontFamily(Fonts.TimesRoman).FontSize(11));

                page.Content()
                    .Column(column =>
                    {
                        // HEADER (isti kao na prvoj strani)
                        column.Item().Row(row =>
                        {
                            row.RelativeItem(3).Column(col =>
                            {
                                if (logoBytes != null)
                                {
                                    col.Item().Width(120).Image(logoBytes).FitWidth();
                                }
                            });
                            row.RelativeItem(7).PaddingLeft(10).Column(col =>
                            {
                                col.Item().Text("SUINS DOO NOVI SAD, BRANKA BAJIĆA 12, 21000 Novi Sad").Bold().FontSize(10);
                                col.Item().Text("PIB: 107024756; Matični broj: 20726695; šifra delatnosti: 5229").FontSize(9);
                                col.Item().Text("Sedište: Novi Sad Branka Bajića 12; tel: 021/6393-416; office@suins.rs").FontSize(9);
                                col.Item().Text("PJ Subotica: +381 (0)24 551-651; fax: +381 (0)24 551-113; e-mail: subotica@suins.rs").FontSize(9);
                                col.Item().Text("PJ Horgoš: +381 69 795 014; horgoš@suins.rs").FontSize(9);
                                col.Item().Text("PJ Kelebija: +381 (0)24 789 071").FontSize(9);
                            });
                        });

                        column.Item().PaddingTop(4).AlignCenter().Text("www.suins.rs").FontSize(12);
                        column.Item().LineHorizontal(2).LineColor(Colors.Black);
                        column.Item().PaddingVertical(5);

                        column.Item().AlignCenter().Text($"Nalog za transport: {nalog.NalogBroj ?? ""}").Bold().FontSize(14);

                        column.Item().PaddingTop(5);

                        // FINANSIJSKI PODACI
                        // Format price with currency - append "+ Terminali + PDV" only for RSD
                        var cena = nalog.Tura?.UlaznaCena?.ToString("N2") ?? "";
                        var valuta = (nalog.Tura?.Valuta ?? "").Trim();
                        var isRsd = valuta.ToUpper() == "RSD";
                        var cenaTransporta = string.IsNullOrWhiteSpace(cena) 
                            ? valuta 
                            : $"{cena} {valuta}" + (isRsd ? " + Terminali + PDV" : "");

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(7);
                            });

                            AddTableRowSmall(table, "Cena transporta:", cenaTransporta);
                            AddTableRowSmall(table, "Valuta plaćanja:", 
                                "60 dana od dana prijema originalne overene dokumentacije (CMR,EX)");
                            AddTableRowSmall(table, "Način plaćanja:", 
                                "Plaćanje po srednjem kursu NBS-a");
                            AddTableRowSmall(table, "Napomena:", 
                                "Faktura će preko SEF biti prihvaćena samo ukoliko sadrži dokaz o izvršenoj usluzi i svu neophodnu dokumentaciju. " +
                                "Obavezno je upisivanje broja naloga u polje Order Reference.\n\n" +
                                "Rok za dostavu dokumentacije je 15 dana od datuma istovara. U slučaju kašnjenja sa istom, faktura će Vam biti umanjena za 1%.");
                        });

                        column.Item().PaddingTop(10);

                        // USLOVI
                        column.Item().Text("Primalac naloga prihvata sledeće uslove:").Bold();
                        column.Item().PaddingTop(3).Text("1. Zaštita imena poslovnih partnera mora biti zagarantovana, direktan kontakt sa našim klijentom bez našeg znanja je najstrožije zabranjen;");
                        column.Item().Text("2. Pretovar, dotovar bez našeg znanja nije dozvoljen;");
                        column.Item().Text("3. Eventualna nezgoda, kvar ili bilo kakvo drugo odugovlačenje sa datumom utovara/istovara mora se javiti u pismenoj formi. Vozilo mora imati CMR osiguranje, u suprotnom svi naknadni troškovi ili nezgode idu na Vas. Ukoliko nama odbiju procenat od vozarine prenećemo taj trošak na Vas. CMR osiguranje nam se mora dostaviti pre utovara. U suprotnom se ne priznaje;");
                        column.Item().Text("4. Plaćanje transporta nije moguće bez originala, pečatiranog i potpisanog CMR-a bez ikakvih primedbi od strane primaoca. U slučaju bilo kakvih problema odmah nas kontaktirajte;");
                        column.Item().Text("5. Kamioni imaju 24 sata za utovar i 48 sati za istovar besplatno. Subota posle 16:00 h i nedelja se ne računaju u pomenuto vreme;");
                        column.Item().Text("6. Vozač mora prisustvovati utovaru/istovaru i robu primiti/predati uredno po broju koleata prema dokumentima koji moraju biti uredno potpisani i overeni;");
                        column.Item().Text("7. Vozač je odgovoran za osovinsko opterećenje vozila i dužan je da vodi računa o pravilnom rasporedu tereta. Za ukupno opterećenje vozila vozač ima pravo na nadoknadu uz originalnu priznanicu;");
                        column.Item().Text("8. Prilikom utovara španeri su obavezni; Vozila na utovar moraju doći ispravna i čista sa ispravnom ceradom;");
                        column.Item().Text("9. Bilo kakvi naknadni dogovori sa isporučiocem/primaocem robe, a mimo našeg naloga i znanja su zabranjeni;");
                        column.Item().Text("10. Bilo kakav nedostatak ili uzorkovanje robe od strane carinika ili špeditera mora biti upisano u CMR, u suprotnom će vozač biti zadužen za svaki nedostatak robe;");
                        column.Item().Text("11. Ako vozač dobije T5 ili TC11 dokument, obavezno ga mora overiti na zadnjoj Evropskoj granici i dostaviti ga nama u originalu; overen EX-1/T1 dokument obavezno dostaviti nama jer je to uslov naplate vozarine;");
                        column.Item().Text("12. U slučaju da bilo koji od navedenih uslova nije zadovoljen, ne možemo Vam garantovati isplatu vozarine ili nekog dela iste.");

                        column.Item().PaddingTop(10);
                        column.Item().Text("U slučaju spora nadležan je sud u Novom Sadu");
                        column.Item().Text("Ovu stranu naloga ne smete pokazivati na utovaru/istovaru!!!");
                        column.Item().Text($"Uslov naplate vozarine    Datum: {nalog.CreatedAt:dd.MM.yyyy}");

                        column.Item().PaddingTop(10);
                        column.Item().LineHorizontal(1).LineColor(Colors.Black);
                        column.Item().PaddingTop(5).AlignCenter().Text("www.suins.rs").FontSize(12);
                    });
            });
        });
    }

    // ========== TALL TEAM LAYOUT (zasnovan na TallTeamNalogTemplate) ==========
    private static IDocument BuildTallTeamDocument(Nalog nalog, byte[]? logoBytes)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(TextStyle.Default.FontFamily(Fonts.TimesRoman).FontSize(14));

                page.Content().Column(column =>
                {
                    // HEADER – text, nema logo-a u HTML template-u
                    column.Item().AlignCenter().Column(col =>
                    {
                        col.Item().Text("TALL TIM GROUP doo".ToUpper())
                            .FontSize(26)
                            .Bold()
                            .FontColor(Colors.Blue.Medium);

                        col.Item().Text("Alekse Ivića 11; 24000 Subotica\nMB: 20194677 ; PIB: 104599237")
                            .FontSize(13)
                            .FontColor(Colors.Blue.Medium);
                    });

                    column.Item().PaddingTop(4).AlignCenter().Text("www.suins.rs").FontSize(12);
                    column.Item().LineHorizontal(2).LineColor(Colors.Black);
                    column.Item().PaddingVertical(8);

                    column.Item().AlignCenter().Text($"Nalog za transport: {nalog.NalogBroj ?? ""}")
                        .Bold().FontSize(16);

                    column.Item().PaddingTop(10);

                    // DAVALAC / PRIMALAC
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Davalac naloga:").Bold();
                            col.Item().Text("Firma: TALL-TIM GROUP DOO");
                            col.Item().Text("Ime: Stefan Miletić");
                            col.Item().Text("Tel: +381 63 512 425");
                            col.Item().Text("E-mail: transport@suins.rs");
                        });
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Primalac naloga:").Bold();
                            col.Item().Background(Colors.Grey.Lighten3).Text($"Firma: {nalog.Prevoznik?.Naziv ?? ""}");
                            col.Item().Background(Colors.Grey.Lighten3).Text($"Ime: {nalog.Prevoznik?.Kontakt ?? ""}");
                            col.Item().Background(Colors.Grey.Lighten3).Text($"Tel: {nalog.Prevoznik?.Telefon ?? ""}");
                            col.Item().Background(Colors.Grey.Lighten3).Text("E-mail:");
                        });
                    });

                    column.Item().PaddingTop(10);

                    // PODACI NALOGA
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Podaci naloga").Bold();
                        row.RelativeItem().AlignCenter().Text("Predmet: Transport").Bold();
                        row.RelativeItem().AlignRight().Text($"Datum: {nalog.CreatedAt:dd.MM.yyyy}").Bold();
                    });

                    column.Item().PaddingTop(5);

                    // TABELA SA PODACIMA (ista struktura kao Suins/TallTeam HTML)
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                        });

                        AddTableRow(table, "Broj vozila:", nalog.Tura?.Vozilo?.Naziv ?? "");
                        AddTableRow(table, "Registarski broj vozila:", nalog.RegistarskiBrojVozilaExt ?? "");
                        AddTableRow(table, "Relacija:", nalog.Relacija ?? "");
                        AddTableRow(table, "Datum i vreme utovara:",
                            nalog.DatumUtovara?.ToString("dd.MM.yyyy HH:mm") ?? "");
                        AddTableRow(table, "Mesto utovara:", nalog.Tura?.MestoUtovara ?? "");
                        AddTableRow(table, "Adresa utovara:", nalog.AdresaUtovara ?? "");
                        AddTableRow(table, "Količina robe:", 
                            FormatKolicinaITezina(nalog.KolicinaRobe, nalog.Tura?.Tezina));
                        AddTableRow(table, "Vrsta robe:", nalog.VrstaRobe ?? "");
                        AddTableRow(table, "Izvoznik:", nalog.Izvoznik ?? "");
                        AddTableRow(table, "Izvozno carinjenje:", nalog.IzvoznoCarinjenje ?? "");
                        AddTableRow(table, "Granični prelaz ulaska u Srbiju:", nalog.GranicniPrelaz ?? "");
                        AddTableRow(table, "Uvoznik:", nalog.Uvoznik ?? "");
                        AddTableRow(table, "Uvozno carinjenje:", nalog.UvoznoCarinjenje ?? "");
                        AddTableRow(table, "Špedicija:", nalog.Spedicija ?? "");
                        AddTableRow(table, "Datum istovara:",
                            nalog.DatumIstovara?.ToString("dd.MM.yyyy HH:mm") ?? "");
                        AddTableRow(table, "Mesto istovara:", nalog.Tura?.MestoIstovara ?? "");
                        AddTableRow(table, "Adresa istovara:", nalog.AdresaIstovara ?? "");
                    });

                    column.Item().PaddingTop(10);

                    if (!string.IsNullOrWhiteSpace(nalog.NapomenaNalog))
                    {
                        column.Item().Text("Napomena:").Bold();
                        column.Item().Text(nalog.NapomenaNalog ?? "");
                    }

                    column.Item().PaddingTop(10);
                });
            });

            // Druga strana – tekstualni uslovi (isti kao kod Suins/MTS, ali bez header logo-a)
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(TextStyle.Default.FontFamily(Fonts.TimesRoman).FontSize(11));

                page.Content().Column(column =>
                {
                    column.Item().AlignCenter().Column(col =>
                    {
                        col.Item().Text("TALL TIM GROUP doo".ToUpper())
                            .FontSize(26)
                            .Bold()
                            .FontColor(Colors.Blue.Medium);

                        col.Item().Text("Alekse Ivića 11; 24000 Subotica\nMB: 20194677 ; PIB: 104599237")
                            .FontSize(13)
                            .FontColor(Colors.Blue.Medium);
                    });

                    column.Item().PaddingTop(4).AlignCenter().Text("www.suins.rs").FontSize(12);
                    column.Item().LineHorizontal(2).LineColor(Colors.Black);
                    column.Item().PaddingVertical(5);

                    column.Item().AlignCenter().Text($"Nalog za transport: {nalog.NalogBroj ?? ""}")
                        .Bold().FontSize(14);

                    column.Item().PaddingTop(5);

                    // FINANSIJSKI PODACI
                    // Format price with currency - append "+ Terminali + PDV" only for RSD
                    var cena = nalog.Tura?.UlaznaCena?.ToString("N2") ?? "";
                    var valuta = (nalog.Tura?.Valuta ?? "").Trim();
                    var isRsd = valuta.ToUpper() == "RSD";
                    var cenaTransporta = string.IsNullOrWhiteSpace(cena) 
                        ? valuta 
                        : $"{cena} {valuta}" + (isRsd ? " + Terminali + PDV" : "");

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(7);
                        });

                        AddTableRowSmall(table, "Cena transporta:", cenaTransporta);
                        AddTableRowSmall(table, "Valuta plaćanja:",
                            "60 dana od dana prijema originalne overene dokumentacije (CMR,EX)");
                        AddTableRowSmall(table, "Način plaćanja:",
                            "Plaćanje po srednjem kursu NBS-a");
                        AddTableRowSmall(table, "Napomena:",
                            "Faktura će preko SEF biti prihvaćena samo ukoliko sadrži dokaz o izvršenoj usluzi i svu neophodnu dokumentaciju. " +
                            "Obavezno je upisivanje broja naloga u polje Order Reference.\n\n" +
                            "Rok za dostavu dokumentacije je 15 dana od datuma istovara. U slučaju kašnjenja sa istom, faktura će Vam biti umanjena za 1%.");
                    });

                    column.Item().PaddingTop(10);

                    column.Item().Text("Primalac naloga prihvata sledeće uslove:").Bold();
                    column.Item().PaddingTop(3).Text("1. Zaštita imena poslovnih partnera mora biti zagarantovana, direktan kontakt sa našim klijentom bez našeg znanja je najstrožije zabranjen;");
                    column.Item().Text("2. Pretovar, dotovar bez našeg znanja nije dozvoljen;");
                    column.Item().Text("3. Eventualna nezgoda, kvar ili bilo kakvo drugo odugovlačenje sa datumom utovara/istovara mora se javiti u pismenoj formi. Vozilo mora imati CMR osiguranje, u suprotnom svi naknadni troškovi ili nezgode idu na Vas. Ukoliko nama odbiju procenat od vozarine prenećemo taj trošak na Vas. CMR osiguranje nam se mora dostaviti pre utovara. U suprotnom se ne priznaje;");
                    column.Item().Text("4. Plaćanje transporta nije moguće bez originala, pečatiranog i potpisanog CMR-a bez ikakvih primedbi od strane primaoca. U slučaju bilo kakvih problema odmah nas kontaktirajte;");
                    column.Item().Text("5. Kamioni imaju 24 sata za utovar i 48 sati za istovar besplatno. Subota posle 16:00 h i nedelja se ne računaju u pomenuto vreme;");
                    column.Item().Text("6. Vozač mora prisustvovati utovaru/istovaru i robu primiti/predati uredno po broju koleata prema dokumentima koji moraju biti uredno potpisani i overeni;");
                    column.Item().Text("7. Vozač je odgovoran za osovinsko opterećenje vozila i dužan je da vodi računa o pravilnom rasporedu tereta. Za ukupno opterećenje vozila vozač ima pravo na nadoknadu uz originalnu priznanicu;");
                    column.Item().Text("8. Prilikom utovara španeri su obavezni; Vozila na utovar moraju doći ispravna i čista sa ispravnom ceradom;");
                    column.Item().Text("9. Bilo kakvi naknadni dogovori sa isporučiocem/primaocem robe, a mimo našeg naloga i znanja su zabranjeni;");
                    column.Item().Text("10. Bilo kakav nedostatak ili uzorkovanje robe od strane carinika ili špeditera mora biti upisano u CMR, u suprotnom će vozač biti zadužen za svaki nedostatak robe;");
                    column.Item().Text("11. Ako vozač dobije T5 ili TC11 dokument, obavezno ga mora overiti na zadnjoj Evropskoj granici i dostaviti ga nama u originalu; overen EX-1/T1 dokument obavezno dostaviti nama jer je to uslov naplate vozarine;");
                    column.Item().Text("12. U slučaju da bilo koji od navedenih uslova nije zadovoljen, ne možemo Vam garantovati isplatu vozarine ili nekog dela iste.");

                    column.Item().PaddingTop(10);
                    column.Item().Text("U slučaju spora nadležan je sud u Novom Sadu");
                    column.Item().Text("Ovu stranu naloga ne smete pokazivati na utovaru/istovaru!!!");
                    column.Item().Text($"Uslov naplate vozarine    Datum: {nalog.CreatedAt:dd.MM.yyyy}");

                    column.Item().PaddingTop(10);
                    column.Item().LineHorizontal(1).LineColor(Colors.Black);
                    column.Item().PaddingTop(5).AlignCenter().Text("www.suins.rs").FontSize(12);
                });
            });
        });
    }

    private static void AddTableRow(TableDescriptor table, string label, string value)
    {
        table.Cell().Element(LabelCell).Text(label).Bold();
        // Background se već primenjuje u ValueCell (na kontejner)
        table.Cell().Element(ValueCell).Text(value);
    }

    private static void AddTableRowSmall(TableDescriptor table, string label, string value)
    {
        table.Cell().Element(LabelCell).Text(label).Bold().FontSize(11);
        table.Cell().Element(ValueCell).Text(value).FontSize(11);
    }

    private static IContainer LabelCell(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Black)
            .Padding(5)
            .Background(Colors.White);
    }

    private static IContainer ValueCell(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Black)
            .Padding(5)
            .Background(Colors.Grey.Lighten3);
    }

    private static string FormatKolicinaITezina(string? kolicina, string? tezina)
    {
        var hasKolicina = !string.IsNullOrWhiteSpace(kolicina);
        var hasTezina = !string.IsNullOrWhiteSpace(tezina);

        if (hasKolicina && hasTezina)
            return $"{kolicina}; {tezina}";
        
        if (hasKolicina)
            return kolicina;
        
        if (hasTezina)
            return tezina;
        
        return "";
    }
}

