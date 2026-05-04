using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.DataAccess.Models;

[Table("Drzave")]
public class Drzava
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int DrzavaId { get; set; }

    [Required, MaxLength(100)]
    public string Naziv { get; set; } = string.Empty;

    [Required, MaxLength(10)]
    public string Kod { get; set; } = string.Empty;

    public bool Aktivna { get; set; } = true;

    public ICollection<PrevoznikDrzava> PrevoznikDrzave { get; set; } = new List<PrevoznikDrzava>();
}
