using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Utils.DTOs.UserDTO;

public class EmployeeCreateDto
{
    [Required(ErrorMessage = "ID korisnika je obavezan")]
    public int UserId { get; set; }

    [MaxLength(50)]
    public string? EmployeeNumber { get; set; }

    [MaxLength(100)]
    public string? Position { get; set; }

    [MaxLength(100)]
    public string? Department { get; set; }

    public int? PoslovnicaId { get; set; }

    public DateTime? HireDate { get; set; }

    [MaxLength(50)]
    public string? LicenseNumber { get; set; }

    public DateTime? LicenseExpiryDate { get; set; }

    [MaxLength(20)]
    public string? LicenseCategory { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Plata mora biti pozitivan broj")]
    public decimal? Salary { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;
}

