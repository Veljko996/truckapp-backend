namespace WebApplication1.DataAccess.Models;

[Table("TipDokumenta")]
public class TipDokumenta
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TipDokumentaId { get; set; }

    [Required, MaxLength(100)]
    public string Naziv { get; set; } = string.Empty;

    public int SortOrder { get; set; } = 0;

    public ICollection<NalogDokument> NalogDokumenti { get; set; } = new List<NalogDokument>();
}
