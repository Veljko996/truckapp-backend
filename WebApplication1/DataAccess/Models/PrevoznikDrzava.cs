using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.DataAccess.Models;

[Table("PrevoznikDrzave")]
public class PrevoznikDrzava : ITenantEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PrevoznikDrzavaId { get; set; }

    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public int PrevoznikId { get; set; }
    public Prevoznik Prevoznik { get; set; } = null!;

    public int DrzavaId { get; set; }
    public Drzava Drzava { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
