using WebApplication1.Utils.DTOs.NalogPrihodiDTO;

namespace WebApplication1.Utils.Helper;

/// <summary>
/// Deljena finansijska računica po valuti — ista logika koju koristi Krug rezime
/// (KrugService.BuildFinancialSummary), izvučena da je izveštaji mogu reuse-ovati.
/// Nikad se ne mešaju valute; svaka valuta se sabira odvojeno.
/// </summary>
public static class FinancialCalc
{
    public static string NormalizeCurrency(string? currency)
        => string.IsNullOrWhiteSpace(currency) ? "RSD" : currency.Trim().ToUpperInvariant();

    /// <summary>Sabira iznose grupisano po (normalizovanoj) valuti.</summary>
    public static List<AmountByCurrencyDto> BuildTotals(IEnumerable<(string? Currency, decimal Amount)> values)
        => values
            .GroupBy(x => NormalizeCurrency(x.Currency))
            .Select(g => new AmountByCurrencyDto { Currency = g.Key, Amount = g.Sum(x => x.Amount) })
            .OrderBy(x => x.Currency)
            .ToList();

    /// <summary>Profit = prihodi − troškovi, po valuti (uzima uniju svih valuta iz oba).</summary>
    public static List<AmountByCurrencyDto> Profit(
        List<AmountByCurrencyDto> prihodi,
        List<AmountByCurrencyDto> troskovi)
    {
        var valute = prihodi.Select(x => x.Currency)
            .Concat(troskovi.Select(x => x.Currency))
            .Distinct()
            .OrderBy(v => v);

        return valute.Select(v => new AmountByCurrencyDto
        {
            Currency = v,
            Amount = (prihodi.FirstOrDefault(x => x.Currency == v)?.Amount ?? 0m)
                   - (troskovi.FirstOrDefault(x => x.Currency == v)?.Amount ?? 0m)
        }).ToList();
    }

    /// <summary>Roll-up: spaja više listi po valuti u jednu (za zbir preko više krugova/vozila).</summary>
    public static List<AmountByCurrencyDto> Merge(IEnumerable<List<AmountByCurrencyDto>> parts)
        => BuildTotals(parts.SelectMany(p => p).Select(a => ((string?)a.Currency, a.Amount)));
}
