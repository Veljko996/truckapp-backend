
namespace WebApplication1.DataAccess.Models;

public class Employee
{
    [Key]
    public int EmployeeId { get; set; }
    
    // 1:1 relationship with User
    [Required]
    [ForeignKey("User")]
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    // Employee specific information
    [MaxLength(50)]
    public string? EmployeeNumber { get; set; } // Broj zaposlenog (JMBG ili slično)
    
    [MaxLength(100)]
    public string? Position { get; set; } // Pozicija (Vozač, Dispečer, Administrator, itd.)
    
    [MaxLength(100)]
    public string? Department { get; set; } // Odeljenje
    
    [ForeignKey("PoslovnicaId")]
    public int? PoslovnicaId { get; set; } // Povezanost sa poslovnicom
    public Poslovnica? Poslovnica { get; set; }
    
    public DateTime? HireDate { get; set; } // Datum zaposlenja
    
    public DateTime? TerminationDate { get; set; } // Datum prestanka radnog odnosa
    
    [MaxLength(50)]
    public string? LicenseNumber { get; set; } // Broj vozačke dozvole (za vozače)
    
    public DateTime? LicenseExpiryDate { get; set; } // Datum isteka vozačke
    
    [MaxLength(20)]
    public string? LicenseCategory { get; set; } // Kategorija vozačke (B, C, CE, itd.)
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? Salary { get; set; } // Plata (opciono)
    
    [MaxLength(500)]
    public string? Notes { get; set; } // Dodatne napomene
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsActive { get; set; } = true;
}

