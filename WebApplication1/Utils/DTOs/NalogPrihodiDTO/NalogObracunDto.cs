using WebApplication1.Utils.DTOs.NalogTroskoviDTO;

namespace WebApplication1.Utils.DTOs.NalogPrihodiDTO;

public class NalogObracunDto
{
    public int NalogId { get; set; }
    public List<NalogPrihodDto> Prihodi { get; set; } = new();
    public List<NalogTrosakDto> Troskovi { get; set; } = new();
    public decimal UkupniPrihodi { get; set; }
    public decimal UkupniTroskovi { get; set; }
    public decimal Profit { get; set; }
}
