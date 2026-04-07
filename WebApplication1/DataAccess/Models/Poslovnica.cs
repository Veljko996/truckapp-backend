using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DataAccess.Models;

public class Poslovnica : ITenantEntity
{
    [Key]
    public int PoslovnicaId { get; set; }       // Primarni ključ

    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public string PJ { get; set; } = null!;     // Naziv poslovne jedinice
    public string Lokacija { get; set; } = null!;
    public string BrojTelefona { get; set; } = null!;
    public string Email { get; set; } = null!;
    
    // Navigation property
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
