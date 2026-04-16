using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WebApplication1.DataAccess.Models;

namespace WebApplication1.Services.QuestPdfServices;

public partial class QuestPdfNalogGenerator
{
    private static IDocument BuildFeticoDocument(Nalog nalog, byte[]? logoBytes)
    {
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
                        column.Item().Row(row =>
                        {
                            row.RelativeItem(3).Column(col =>
                            {
                                if (logoBytes != null)
                                    col.Item().Width(120).Image(logoBytes).FitWidth();
                            });
                            row.RelativeItem(7).PaddingLeft(10).Column(col =>
                            {
                                col.Item().Text("PR ALEKSANDAR JOVANOVIĆ FETICO MILK & LOGISTIC").Bold().FontSize(11);
                                col.Item().Text("Matije Gupca 107, 26232 Starčevo").FontSize(10);
                                col.Item().Text("PIB 109267361, MB 64054970, Timocom ID: 448381").FontSize(10);
                                col.Item().Text("Kontakt tel: Aleksandar: +381 65 538 2429, Željko: +381 61 151 5360").FontSize(9);
                                col.Item().Text("Email: feticomilk@gmail.com, alexandar@feticomilk.com, zeljko@feticoevropa.com").FontSize(8);
                            });
                        });

                        column.Item().PaddingTop(6);
                        column.Item().LineHorizontal(2).LineColor(Colors.Black);
                        column.Item().PaddingVertical(8);

                        column.Item().AlignCenter().Text($"Nalog za transport: {nalog.NalogBroj ?? ""}").Bold().FontSize(16);
                        column.Item().PaddingTop(10);

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Davalac naloga:").Bold();
                                col.Item().Text("Firma: PR ALEKSANDAR JOVANOVIĆ FETICO MILK & LOGISTIC");
                                col.Item().Text("Matije Gupca 107, 26232 Starčevo");
                                col.Item().Text("Kontakt: Aleksandar / Željko (vidi zaglavlje)");
                                col.Item().Text("E-mail: feticomilk@gmail.com");
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

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Podaci naloga").Bold();
                            row.RelativeItem().AlignCenter().Text("Predmet: Transport").Bold();
                            row.RelativeItem().AlignRight().Text($"Datum: {nalog.CreatedAt:dd.MM.yyyy}").Bold();
                        });

                        column.Item().PaddingTop(5);

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

            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(TextStyle.Default.FontFamily(Fonts.TimesRoman).FontSize(11));

                page.Content()
                    .Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem(3).Column(col =>
                            {
                                if (logoBytes != null)
                                    col.Item().Width(120).Image(logoBytes).FitWidth();
                            });
                            row.RelativeItem(7).PaddingLeft(10).Column(col =>
                            {
                                col.Item().Text("PR ALEKSANDAR JOVANOVIĆ FETICO MILK & LOGISTIC").Bold().FontSize(11);
                                col.Item().Text("Matije Gupca 107, 26232 Starčevo").FontSize(10);
                                col.Item().Text("PIB 109267361, MB 64054970").FontSize(10);
                            });
                        });

                        column.Item().PaddingTop(4);
                        column.Item().LineHorizontal(2).LineColor(Colors.Black);
                        column.Item().PaddingVertical(5);

                        column.Item().AlignCenter().Text($"Nalog za transport: {nalog.NalogBroj ?? ""}").Bold().FontSize(14);
                        column.Item().PaddingTop(5);

                        var cena = nalog.Tura?.UlaznaCena?.ToString("N2") ?? "";
                        var valuta = (nalog.Tura?.Valuta ?? "").Trim();
                        var isRsd = valuta.ToUpperInvariant() == "RSD";
                        var cenaTransporta = string.IsNullOrWhiteSpace(cena)
                            ? valuta
                            : $"{cena} {valuta}" + (isRsd ? " (PDV i dodaci po ponudi)" : "");

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(7);
                            });

                            AddTableRowSmall(table, "Cena transporta:", cenaTransporta);
                            AddTableRowSmall(table, "Valuta plaćanja:", FeticoNalogPdfText.ValutaPlacanja);
                            AddTableRowSmall(table, "Način plaćanja:", FeticoNalogPdfText.NacinPlacanja);
                            AddTableRowSmall(table, "Napomena:", FeticoNalogPdfText.NapomenaFinansije);
                        });

                        column.Item().PaddingTop(8);
                        column.Item().Text("Primalac naloga prihvata sledeće uslove:").Bold().FontSize(10);

                        for (var i = 0; i < FeticoNalogPdfText.UsloviSr.Length; i++)
                        {
                            column.Item().PaddingTop(2).Text($"{i + 1}. {FeticoNalogPdfText.UsloviSr[i]}").FontSize(8.5f);
                        }

                        column.Item().PaddingTop(8);
                        column.Item().Text("U slučaju spora nadležan je sud u Republici Srbiji.").FontSize(10);
                        column.Item().Text("Ovu stranu naloga ne smete pokazivati na utovaru/istovaru bez odobrenja nalagodavca.").FontSize(10);
                        column.Item().Text($"Datum: {nalog.CreatedAt:dd.MM.yyyy}").FontSize(10);

                        column.Item().PaddingTop(8);
                        column.Item().LineHorizontal(1).LineColor(Colors.Black);
                    });
            });

            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(TextStyle.Default.FontFamily(Fonts.TimesRoman).FontSize(10));

                page.Content()
                    .Column(column =>
                    {
                        column.Item().Text("Napomena – fakturisanje").Bold().FontSize(12);
                        column.Item().PaddingTop(6).Text(FeticoNalogPdfText.UputstvoFaktureSr).FontSize(9);

                        column.Item().PaddingTop(20);
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("NALAGODAVAC: _________________________");
                            row.RelativeItem().Text("NALAGOPRIMAC: _________________________");
                        });

                        column.Item().PaddingTop(16);
                        column.Item().Text(FeticoNalogPdfText.FooterPoslednjaNapomena).FontSize(9).Italic();
                    });
            });
        });
    }
}
