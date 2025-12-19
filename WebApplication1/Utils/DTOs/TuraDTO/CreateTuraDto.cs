using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Utils.DTOs.TuraDTO;

public class CreateTuraDto
{
    [Required]
    [MaxLength(150)]
    public string MestoUtovara { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string MestoIstovara { get; set; } = string.Empty;

    public DateTime? DatumUtovaraOd { get; set; }
    public DateTime? DatumUtovaraDo { get; set; }
    public DateTime? DatumIstovaraOd { get; set; }
    public DateTime? DatumIstovaraDo { get; set; }

    [MaxLength(100)]
    public string? KolicinaRobe { get; set; }

    [MaxLength(50)]
    public string? Tezina { get; set; }

    [Required]
    public int VrstaNadogradnjeId { get; set; }
    [Required]
    public int KlijentId { get; set; }
    [Required]
    public int PrevoznikId { get; set; }
    public int? VoziloId { get; set; }
    

}
