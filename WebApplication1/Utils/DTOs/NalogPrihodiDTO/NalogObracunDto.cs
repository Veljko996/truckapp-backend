using WebApplication1.Utils.DTOs.NalogTroskoviDTO;

namespace WebApplication1.Utils.DTOs.NalogPrihodiDTO;

public class NalogObracunDto
{
    public int NalogId { get; set; }
    public List<NalogPrihodDto> Prihodi { get; set; } = new();
    public List<NalogTrosakDto> Troskovi { get; set; } = new();
    public List<AmountByCurrencyDto> UkupniPrihodiPoValuti { get; set; } = new();
    public List<AmountByCurrencyDto> UkupniTroskoviPoValuti { get; set; } = new();
    public List<AmountByCurrencyDto> ProfitPoValuti { get; set; } = new();
}
