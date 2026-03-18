using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Utils.DTOs.DriverAssignmentDTO;

public class AssignDriverDto
{
    [Required]
    public int EmployeeId { get; set; }

    [Range(1, 2, ErrorMessage = "Slot mora biti 1 ili 2.")]
    public int SlotNumber { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }
}

