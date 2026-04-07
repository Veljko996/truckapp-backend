using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.DataAccess.Models;

[Table("NasaVoziloVozacAssignments")]
public class NasaVoziloVozacAssignment : ITenantEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AssignmentId { get; set; }

    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public int VoziloId { get; set; }
    public NasaVozila? Vozilo { get; set; }

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public int SlotNumber { get; set; } // 1 ili 2

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UnassignedAt { get; set; }

    [MaxLength(200)]
    public string? AssignedBy { get; set; }

    [MaxLength(200)]
    public string? UnassignedBy { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }
}

