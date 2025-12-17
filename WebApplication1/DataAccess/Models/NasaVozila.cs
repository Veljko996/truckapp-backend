using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.DataAccess.Models;

[Table("NasaVozila")]
public class NasaVozila
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int VoziloId { get; set; }

    [Required, MaxLength(100)]
    public string Naziv { get; set; } = string.Empty; // npr. Scania R500

    [MaxLength(80)]
    public string? TipVozila { get; set; } // do 3.5t, 7.5t, 12t, šleper, itd.

    [MaxLength(100)]
    public string? MarkaModel { get; set; }

    public bool RegistrovanoVozilo { get; set; } = true;
    public DateTime? RegistracijaDatumIsteka { get; set; }
    public DateTime? TehnickiPregledDatumIsteka { get; set; }
    public DateTime? PPAparatDatumIsteka { get; set; }

    [MaxLength(100)]
    public string? Raspolozivost { get; set; } = "Slobodno"; 

    [MaxLength(100)]
    public string? Relacija { get; set; } 

    // Navigacije
    public ICollection<Vinjeta> Vinjete { get; set; } = new List<Vinjeta>();
    public ICollection<Tura> Ture { get; set; } = new List<Tura>();

    /// <summary>
    /// Auto-updates registration status based on expiration date.
    /// </summary>
    public void AzurirajStatusRegistracije()
    {
        if (!RegistracijaDatumIsteka.HasValue)
        {
            RegistrovanoVozilo = false;
            return;
        }

        RegistrovanoVozilo = RegistracijaDatumIsteka.Value >= DateTime.UtcNow;
    }
    public bool HasActiveVignettes(DateTime? referenceDate = null)
    {
        var now = referenceDate ?? DateTime.UtcNow;
        return Vinjete.Any(v => v.IsActive(now));
    }

}
