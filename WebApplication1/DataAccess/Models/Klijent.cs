namespace WebApplication1.DataAccess.Models;

public class Klijent
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int KlijentId { get; set; }

    [Required]
    [MaxLength(200)]
    public string NazivFirme { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Drzava { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Grad { get; set; }

    [MaxLength(255)]
    public string? Adresa { get; set; }

    [MaxLength(50)]
    public string? PIB { get; set; }

    [MaxLength(50)]
    public string? PoreskiBroj { get; set; }

    [MaxLength(150)]
    public string? KontaktOsoba { get; set; }

    [MaxLength(50)]
    public string? Telefon { get; set; }

    [MaxLength(150)]
    public string? Email { get; set; }

    [MaxLength(350)]
    public string? Napomena { get; set; }

    public bool Aktivan { get; set; }

    public DateTime DatumKreiranja { get; set; }
    public DateTime? DatumAzuriranja { get; set; }
    
}
