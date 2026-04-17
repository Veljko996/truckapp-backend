using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.DataAccess.Models;

[Table("Krugovi")]
public class Krug : ITenantEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int KrugId { get; set; }

    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    [MaxLength(50)]
    public string? Broj { get; set; }

    [Required]
    public int VoziloId { get; set; }
    public NasaVozila? Vozilo { get; set; }

    public DateTime StartAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndAt { get; set; }

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Otvoren"; // Otvoren | Zatvoren

    [MaxLength(500)]
    public string? Napomena { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(200)]
    public string? CreatedBy { get; set; }

    public DateTime? ClosedAt { get; set; }

    [MaxLength(200)]
    public string? ClosedBy { get; set; }

    public ICollection<Tura> Ture { get; set; } = new List<Tura>();
    public ICollection<KrugTrosak> Troskovi { get; set; } = new List<KrugTrosak>();
}
