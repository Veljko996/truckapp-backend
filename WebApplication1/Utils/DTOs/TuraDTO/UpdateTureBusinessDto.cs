namespace WebApplication1.Utils.DTOs.TuraDTO;
public class UpdateTureBusinessDto
{

    public int? KlijentId { get; set; }
    public int? PrevoznikId { get; set; }
    public int? VoziloId { get; set; }

    public decimal? UlaznaCena { get; set; }
    public decimal? IzlaznaCena { get; set; }

    public string? Valuta { get; set; }

}
