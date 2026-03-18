namespace WebApplication1.Utils.DTOs.DriverAssignmentDTO;

public class DriverAssignmentReadDto
{
    public int AssignmentId { get; set; }
    public int VoziloId { get; set; }
    public int EmployeeId { get; set; }
    public int SlotNumber { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? UnassignedAt { get; set; }
    public string? AssignedBy { get; set; }
    public string? UnassignedBy { get; set; }
    public string? Note { get; set; }

    public string? EmployeeFullName { get; set; }
    public string? EmployeeNumber { get; set; }
    public string? Position { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
}

