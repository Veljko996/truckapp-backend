using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.DataAccess.Models;

[Table("TuraStatusLog")]
public class TuraStatusLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int StatusLogId { get; set; }

    public int TuraId { get; set; }
    public Tura Tura { get; set; } = null!;

    [Required, MaxLength(50)]
    public string Status { get; set; } = string.Empty;

    [Required]
    public DateTime Vreme { get; set; } = DateTime.UtcNow;

    [MaxLength(300)]
    public string? Napomena { get; set; }

    public string? UserName { get; set; }
}