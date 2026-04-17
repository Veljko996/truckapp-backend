using WebApplication1.Utils.DTOs.NalogDTO;
using WebApplication1.Utils.DTOs.KrugTroskoviDTO;
using WebApplication1.Utils.DTOs.NalogPrihodiDTO;

namespace WebApplication1.Utils.DTOs.KrugDTO;

public class KrugDetailsDto
{
    public int KrugId { get; set; }
    public string? Broj { get; set; }
    public int VoziloId { get; set; }
    public string? VoziloNaziv { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Napomena { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? ClosedBy { get; set; }

    public List<KrugTuraItemDto> Ture { get; set; } = new();
    public List<KrugTrosakDto> Troskovi { get; set; } = new();

    public List<AmountByCurrencyDto> UkupniTroskoviKrugaPoValuti { get; set; } = new();
    public List<AmountByCurrencyDto> UkupniTroskoviNalogaPoValuti { get; set; } = new();
    public List<AmountByCurrencyDto> UkupniPrihodiPoValuti { get; set; } = new();
    public List<AmountByCurrencyDto> ProfitPoValuti { get; set; } = new();
}

public class KrugTuraItemDto
{
    public int TuraId { get; set; }
    public string? RedniBroj { get; set; }
    public string MestoUtovara { get; set; } = string.Empty;
    public string MestoIstovara { get; set; } = string.Empty;
    public DateTime? DatumUtovara { get; set; }
    public DateTime? DatumIstovara { get; set; }
    public string? StatusTure { get; set; }
    public string? KlijentNaziv { get; set; }
    public string? PrevoznikNaziv { get; set; }
    public bool? PrevoznikInterni { get; set; }

    public NalogReadDto? Nalog { get; set; }
}
