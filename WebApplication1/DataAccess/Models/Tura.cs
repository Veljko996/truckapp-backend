using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Utils.Enums;

namespace WebApplication1.DataAccess.Models;

public class Tura
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TuraId { get; set; }
    [Required, MaxLength(50)]
    public string RedniBroj { get; set; } = string.Empty;
    [Required, MaxLength(200)]
    public string Relacija { get; set; } = string.Empty;
    [Required]
    public DateTime UtovarDatum { get; set; }
    [Required]
    public DateTime IstovarDatum { get; set; }
    [MaxLength(50)]
    public string? KolicinaRobe { get; set; }
    [MaxLength(50)]
    public string? Tezina { get; set; }
    [Required, MaxLength(20)]
    public string OpcijaPrevoza { get; set; } = "Solo";
    [MaxLength(100)]
    public string? VrstaNadogradnje { get; set; }
    [MaxLength(500)]
    public string? Napomena { get; set; }
    public decimal? UlaznaCena { get; set; }

    // Statusi
    [Required, MaxLength(50)]
    public string StatusTrenutni { get; set; } = TuraStatus.UPripremi;
    public DateTime? StatusTrenutniVreme { get; set; }
    [Required, MaxLength(50)]
    public string StatusKonacni { get; set; } = TuraFinalStatus.UObradi;

    /// <summary>
    /// Updates the current status and sets the timestamp. Domain method to ensure consistency.
    /// </summary>
    public void UpdateStatus(string newStatus, string? napomena = null)
    {
        if (string.IsNullOrWhiteSpace(newStatus))
            throw new ArgumentException("Status cannot be null or empty.", nameof(newStatus));

        StatusTrenutni = newStatus;
        StatusTrenutniVreme = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the tour is in a final state (completed or cancelled).
    /// </summary>
    public bool IsFinalState() => StatusTrenutni == TuraStatus.Zavrseno || StatusTrenutni == TuraStatus.Otkazano;

    /// <summary>
    /// Validates that critical fields cannot be changed when in final state.
    /// </summary>
    public void ValidateCanModify()
    {
        if (IsFinalState())
            throw new InvalidOperationException($"Cannot modify tour in final state: {StatusTrenutni}");
    }
    public int PrevoznikId { get; set; } 
    public Prevoznik Prevoznik { get; set; } = null!;

    // Vozilo (optional - can be assigned later)
    public int? VoziloId { get; set; }
    public NasaVozila? Vozilo { get; set; }

    // Logovi
    public List<TuraStatusLog> StatusLogovi { get; set; } = new();
}