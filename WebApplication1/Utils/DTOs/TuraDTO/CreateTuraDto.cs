using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Utils.DTOs.TuraDTO;

public class CreateTuraDto
{
    [Required]
    public string MestoUtovara { get; set; } = string.Empty;
    [Required]
    public string MestoIstovara { get; set; } = string.Empty;
    public DateTime? DatumUtovara { get; set; }
    public DateTime? DatumIstovara { get; set; }
    public string? KolicinaRobe { get; set; }
    public string? Tezina { get; set; }
    [Required]
    public int VrstaNadogradnjeId { get; set; }
    [Required]
    public int KlijentId { get; set; }
    [Required]
    public int PrevoznikId { get; set; }
    public int? VoziloId { get; set; }

}
