
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
    
    public string MestoUtovara { get; set; } = string.Empty;

    [Required]
    
    public string MestoIstovara { get; set; } = string.Empty;

    public DateTime? DatumUtovara { get; set; }
    public DateTime? DatumIstovara { get; set; }


    public string? KolicinaRobe { get; set; }

    public string? Tezina { get; set; }

    [MaxLength(350)]
    public string? NapomenaKlijenta { get; set; }
    public string? Napomena { get; set; }
    // Carinjenje – dodatno u detail fazi

    public string? IzvoznoCarinjenje { get; set; }


    public string? UvoznoCarinjenje { get; set; }

    // Finansije – detail faza
    [Column(TypeName = "decimal(12,2)")]
    public decimal? UlaznaCena { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal? IzlaznaCena { get; set; }

    [MaxLength(10)]
    public string? Valuta { get; set; } = "RSD";

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
