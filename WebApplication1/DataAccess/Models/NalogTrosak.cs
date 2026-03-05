using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.DataAccess.Models;

[Table("NalogTroskovi")]
public class NalogTrosak
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TrosakId { get; set; }

    public int NalogId { get; set; }
    public Nalog? Nalog { get; set; }

    public int TipTroskaId { get; set; }
    public TipTroska? TipTroska { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Iznos { get; set; }

    [MaxLength(500)]
    public string? Napomena { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(200)]
    public string? CreatedBy { get; set; }
}
