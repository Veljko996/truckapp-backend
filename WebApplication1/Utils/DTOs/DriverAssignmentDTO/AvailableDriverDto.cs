namespace WebApplication1.Utils.DTOs.DriverAssignmentDTO;

public class AvailableDriverDto
{
    public int EmployeeId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? EmployeeNumber { get; set; }
    public string? Position { get; set; }
    public bool IsAssignedElsewhere { get; set; }
    public int? AssignedVoziloId { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
}

