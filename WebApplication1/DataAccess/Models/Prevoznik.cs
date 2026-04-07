using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.DataAccess.Models;

[Table("Prevoznici")]
public class Prevoznik : ITenantEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PrevoznikId { get; set; }

    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    [Required, MaxLength(100)]
    public string Naziv { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Kontakt { get; set; }

    [MaxLength(50)]
    public string? Telefon { get; set; }

    [MaxLength(20)]
    public string? PIB { get; set; }

    public bool Interni { get; set; }
}
