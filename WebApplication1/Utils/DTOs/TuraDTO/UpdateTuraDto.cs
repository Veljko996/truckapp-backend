using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Utils.DTOs.TuraDTO;

public class UpdateTuraDto
{
    [Required]
    [MaxLength(50)]
    public string RedniBroj { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string MestoUtovara { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string MestoIstovara { get; set; } = string.Empty;

    public DateTime? DatumUtovaraOd { get; set; }
    public DateTime? DatumUtovaraDo { get; set; }
    public DateTime? DatumIstovaraOd { get; set; }
    public DateTime? DatumIstovaraDo { get; set; }

    [MaxLength(100)]
    public string? KolicinaRobe { get; set; }

    [MaxLength(50)]
    public string? Tezina { get; set; }

    [Required]
    [MaxLength(50)]
    public string OpcijaPrevoza { get; set; } = string.Empty;

    [Required]
    public int VrstaNadogradnjeId { get; set; }

    [MaxLength(500)]
    public string? Napomena { get; set; }

    [MaxLength(350)]
    public string? NapomenaKlijenta { get; set; }

    [MaxLength(200)]
    public string? IzvoznoCarinjenje { get; set; }

    [MaxLength(200)]
    public string? UvoznoCarinjenje { get; set; }

    public decimal? UlaznaCena { get; set; }
    public decimal? IzlaznaCena { get; set; }

    [MaxLength(10)]
    public string Valuta { get; set; } = "EUR";

    [Required]
    public int KlijentId { get; set; }

    [Required]
    public int PrevoznikId { get; set; }

    public int? VoziloId { get; set; }

    [Required]
    [MaxLength(50)]
    public string StatusTure { get; set; } = string.Empty;
}
