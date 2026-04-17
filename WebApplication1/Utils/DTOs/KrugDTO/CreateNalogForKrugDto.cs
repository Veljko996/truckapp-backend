using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Utils.DTOs.KrugDTO;

/// <summary>
/// Unified command: kreira Tura, postavi KrugId na Tura, ensure-uj interni Nalog za Turu.
/// Vozilo se uvek povlači iz Kruga (Krug.VoziloId).
/// Prevoznik mora biti interni (validira service).
/// </summary>
public class CreateNalogForKrugDto
{
    [Required]
    public string MestoUtovara { get; set; } = string.Empty;

    [Required]
    public string MestoIstovara { get; set; } = string.Empty;

    public DateTime? DatumUtovara { get; set; }
    public DateTime? DatumIstovara { get; set; }

    public string? KolicinaRobe { get; set; }
    public string? Tezina { get; set; }

    [Required]
    public int VrstaNadogradnjeId { get; set; }

    [Required]
    public int KlijentId { get; set; }

    [Required]
    public int PrevoznikId { get; set; }

    // Polja potrebna za interni nalog (validacija u service-u)
    public decimal? IzlaznaCena { get; set; }
    public decimal? UlaznaCena { get; set; }
    public string? Valuta { get; set; }

    public string? IzvoznoCarinjenje { get; set; }
    public string? UvoznoCarinjenje { get; set; }
    public string? Napomena { get; set; }
    public string? NapomenaKlijenta { get; set; }
}
