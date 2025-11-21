namespace WebApplication1.Utils.DTOs.UserDTO;

public class EmployeeReadDto
{
    public int EmployeeId { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? EmployeeNumber { get; set; }
    public string? Position { get; set; }
    public string? Department { get; set; }
    public int? PoslovnicaId { get; set; }
    public string? PoslovnicaNaziv { get; set; }
    public DateTime? HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public string? LicenseNumber { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public string? LicenseCategory { get; set; }
    public decimal? Salary { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}

