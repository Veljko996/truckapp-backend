using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.DataAccess.Models;

[Table("TipTroska")]
public class TipTroska
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TipTroskaId { get; set; }

    [Required, MaxLength(200)]
    public string Naziv { get; set; } = string.Empty;

    public ICollection<NalogTrosak> NalogTroskovi { get; set; } = new List<NalogTrosak>();
}
