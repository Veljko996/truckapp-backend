using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DataAccess.Models;
public class Log
{
    [Key]
    public int LogId { get; set; }

    [Required]
    public DateTime HappenedAtDate { get; set; } = DateTime.Now;

    [MaxLength(100)]
    public string? Process { get; set; }

    [MaxLength(100)]
    public string? Activity { get; set; } 

    [MaxLength(500)]
    [Required]
    public string Message { get; set; } = string.Empty;

    public int? UserId { get; set; }
    public User? User { get; set; }

    public string? IpAddress { get; set; }

    public int? EntityId { get; set; }
}
