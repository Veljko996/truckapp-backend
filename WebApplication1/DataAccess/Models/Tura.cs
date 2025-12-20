using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.DataAccess.Models;

public class Tura
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TuraId { get; set; }

    // Automatski dodeljuje backend nakon insert-a
    [MaxLength(50)]
    public string? RedniBroj { get; set; }

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

    [MaxLength(350)]
    public string? NapomenaKlijenta { get; set; }
    public string? Napomena { get; set; }
    // Carinjenje – dodatno u detail fazi
    [MaxLength(200)]
    public string? IzvoznoCarinjenje { get; set; }

    [MaxLength(200)]
    public string? UvoznoCarinjenje { get; set; }

    // Finansije – detail faza
    [Column(TypeName = "decimal(12,2)")]
    public decimal? UlaznaCena { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal? IzlaznaCena { get; set; }

    [MaxLength(10)]
    public string? Valuta { get; set; } = "EUR";

    [MaxLength(50)]
    public string StatusTure { get; set; } = "Kreirana";

    public bool KreiranPutniNalog { get; set; } = false;

    // --- FK opcioni dok se ne popune u sledećim fazama ---

    public int? KlijentId { get; set; }
    public Klijent? Klijent { get; set; }

    public int? PrevoznikId { get; set; }
    public Prevoznik? Prevoznik { get; set; }

    public int? VoziloId { get; set; }
    public NasaVozila? Vozilo { get; set; }

    [Required]
    public int VrstaNadogradnjeId { get; set; }
    public VrstaNadogradnje VrstaNadogradnje { get; set; } = null!;
}
