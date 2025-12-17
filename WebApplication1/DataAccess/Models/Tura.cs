using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.DataAccess.Models;

public class Tura
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TuraId { get; set; }

    // Broj ture (npr. 42/25 prikazuješ u UI, ovde čuvaš string za sada)
    [Required]
    [MaxLength(50)]
    public string RedniBroj { get; set; } = string.Empty;

    // Roba – NAMERNO slobodan unos
    [MaxLength(100)]
    public string? KolicinaRobe { get; set; }

    [MaxLength(50)]
    public string? Tezina { get; set; }

    // Solo / Sleper / itd. (za sada string)
    [Required]
    [MaxLength(50)]
    public string OpcijaPrevoza { get; set; } = string.Empty;

    // Napomene
    [MaxLength(500)]
    public string? Napomena { get; set; }

    [MaxLength(350)]
    public string? NapomenaKlijenta { get; set; }

    // Lokacije
    [Required]
    [MaxLength(150)]
    public string MestoUtovara { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string MestoIstovara { get; set; } = string.Empty;

    // Datumi (od–do)
    public DateTime? DatumUtovaraOd { get; set; }
    public DateTime? DatumUtovaraDo { get; set; }
    public DateTime? DatumIstovaraOd { get; set; }
    public DateTime? DatumIstovaraDo { get; set; }

    // Carinjenje – slobodan unos
    [MaxLength(200)]
    public string? IzvoznoCarinjenje { get; set; }

    [MaxLength(200)]
    public string? UvoznoCarinjenje { get; set; }

    // Finansije
    [Column(TypeName = "decimal(12,2)")]
    public decimal? UlaznaCena { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal? IzlaznaCena { get; set; }

    [Required]
    [MaxLength(10)]
    public string Valuta { get; set; } = "EUR";

    // Status ture (JEDAN status)
    [Required]
    [MaxLength(50)]
    public string StatusTure { get; set; } = string.Empty;

    // Putni nalog
    public bool KreiranPutniNalog { get; set; }

    // === FK veze ===

    [Required]
    public int KlijentId { get; set; }
    public Klijent Klijent { get; set; } = null!;

    [Required]
    public int PrevoznikId { get; set; }
    public Prevoznik Prevoznik { get; set; } = null!;

    // Vozilo – samo ako je interni prevoznik
    public int? VoziloId { get; set; }
    public NasaVozila? Vozilo { get; set; }

    // Lookup
    [Required]
    public int VrstaNadogradnjeId { get; set; }
    public VrstaNadogradnje VrstaNadogradnje { get; set; } = null!;
}
