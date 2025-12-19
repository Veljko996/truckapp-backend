using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Utils.DTOs.TuraDTO;

public class UpdateTuraDto
{
    public string? MestoUtovara { get; set; }
    public string? MestoIstovara { get; set; }
    public DateTime? DatumUtovaraOd { get; set; }
    public DateTime? DatumUtovaraDo { get; set; }
    public DateTime? DatumIstovaraOd { get; set; }
    public DateTime? DatumIstovaraDo { get; set; }
    public string? KolicinaRobe { get; set; }
    public string? Tezina { get; set; }
    public int? VrstaNadogradnjeId { get; set; }

}
