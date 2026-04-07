using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DataAccess.Models;

[Table("Tenants")]
public class Tenant
{
    [Key]
    public int TenantId { get; set; }

    [Required, MaxLength(200)]
    public string Naziv { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PIB { get; set; }

    [MaxLength(200)]
    public string? Email { get; set; }

    [MaxLength(50)]
    public string? Telefon { get; set; }

    [MaxLength(300)]
    public string? Adresa { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
