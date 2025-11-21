namespace WebApplication1.Utils.DTOs.VinjetaDTO;

public class VinjetaCreateDTO
{
    public string DrzavaKod { get; set; } = string.Empty;
    public string? Drzava { get; set; }

    public DateTime DatumPocetka { get; set; }
    public DateTime DatumIsteka { get; set; }

    public int? VoziloId { get; set; }
}
