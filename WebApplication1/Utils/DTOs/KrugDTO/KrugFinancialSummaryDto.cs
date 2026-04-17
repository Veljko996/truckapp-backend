using WebApplication1.Utils.DTOs.NalogPrihodiDTO;

namespace WebApplication1.Utils.DTOs.KrugDTO;

public class KrugFinancialSummaryDto
{
    public int KrugId { get; set; }
    public string? Broj { get; set; }
    public string Status { get; set; } = string.Empty;

    public int BrojTura { get; set; }
    public int BrojNaloga { get; set; }

    public List<AmountByCurrencyDto> UkupniTroskoviKrugaPoValuti { get; set; } = new();
    public List<AmountByCurrencyDto> UkupniTroskoviNalogaPoValuti { get; set; } = new();
    public List<AmountByCurrencyDto> UkupniPrihodiPoValuti { get; set; } = new();
    public List<AmountByCurrencyDto> ProfitPoValuti { get; set; } = new();
}
