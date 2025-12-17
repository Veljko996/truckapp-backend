using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.DataAccess.Models;

public class VrstaNadogradnje
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int VrstaNadogradnjeId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Naziv { get; set; } = string.Empty;
}
