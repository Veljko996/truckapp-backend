using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Utils.DTOs.UserDTO;

public class EmployeeUpdateDto
{
    [Required(ErrorMessage = "ID zaposlenog je obavezan")]
    public int EmployeeId { get; set; }

    [MaxLength(50)]
    public string? EmployeeNumber { get; set; }

    [MaxLength(100)]
    public string? Position { get; set; }

    [MaxLength(100)]
    public string? Department { get; set; }

    public int? PoslovnicaId { get; set; }

    public DateTime? HireDate { get; set; }

    public DateTime? TerminationDate { get; set; }

    [MaxLength(50)]
    public string? LicenseNumber { get; set; }

    public DateTime? LicenseExpiryDate { get; set; }

    [MaxLength(20)]
    public string? LicenseCategory { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Plata mora biti pozitivan broj")]
    public decimal? Salary { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public bool? IsActive { get; set; }
}

