using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.DataAccess.Models;

[Table("GorivoZapisi")]
public class GorivoZapis
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int GorivoZapisId { get; set; }

    public int VoziloId { get; set; }
    public NasaVozila? Vozilo { get; set; }

    public int? NalogId { get; set; }
    public Nalog? Nalog { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Iznos { get; set; }

    [Required, MaxLength(10)]
    public string Valuta { get; set; } = "RSD";

    [Column(TypeName = "decimal(10,2)")]
    public decimal KolicineLitara { get; set; }

    public int? Kilometraza { get; set; }

    public DateTime DatumTocenja { get; set; }

    [MaxLength(500)]
    public string? Napomena { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(200)]
    public string? CreatedBy { get; set; }
}
