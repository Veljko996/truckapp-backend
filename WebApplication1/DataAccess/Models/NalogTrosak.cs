using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.DataAccess.Models;

[Table("NalogTroskovi")]
public class NalogTrosak : ITenantEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TrosakId { get; set; }

    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public int NalogId { get; set; }
    public Nalog? Nalog { get; set; }

    public int TipTroskaId { get; set; }
    public TipTroska? TipTroska { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Iznos { get; set; }

    [Required, MaxLength(10)]
    public string Valuta { get; set; } = "RSD";

    [MaxLength(500)]
    public string? Napomena { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(200)]
    public string? CreatedBy { get; set; }
}
