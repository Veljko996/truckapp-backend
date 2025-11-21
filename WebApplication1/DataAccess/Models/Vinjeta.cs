using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.DataAccess.Models;

[Table("Vinjete")]
public class Vinjeta
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int VinjetaId { get; set; }

    [Required, MaxLength(100)]
    public string DrzavaKod { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Drzava { get; set; }

    [Required]
    public DateTime DatumPocetka { get; set; }

    [Required]
    public DateTime DatumIsteka { get; set; }

    public int? VoziloId { get; set; }
    public NasaVozila? Vozilo { get; set; }

    /// <summary>
    /// Checks if the vignette is currently active.
    /// </summary>
    public bool IsActive(DateTime? referenceDate = null)
    {
        var now = referenceDate ?? DateTime.UtcNow;
        return DatumPocetka <= now && DatumIsteka >= now;
    }

    /// <summary>
    /// Checks if this vignette overlaps with another date range.
    /// </summary>
    public bool OverlapsWith(DateTime startDate, DateTime endDate)
    {
        return DatumPocetka <= endDate && DatumIsteka >= startDate;
    }
}
