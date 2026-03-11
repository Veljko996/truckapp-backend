using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.DataAccess.Models;

[Table("NalogPrihodi")]
public class NalogPrihod
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PrihodId { get; set; }

    public int NalogId { get; set; }
    public Nalog? Nalog { get; set; }

    [Required, MaxLength(100)]
    public string TipPrihoda { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Iznos { get; set; }

    [Required, MaxLength(10)]
    public string Valuta { get; set; } = "RSD";

    public bool IsSeededInitial { get; set; } = false;
    public bool IsAutoSyncEnabled { get; set; } = false;

    [MaxLength(500)]
    public string? Napomena { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(200)]
    public string? CreatedBy { get; set; }
}
