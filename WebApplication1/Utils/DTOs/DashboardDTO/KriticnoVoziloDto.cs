namespace WebApplication1.Utils.DTOs.DashboardDTO;

public class KriticnoVoziloDto
{
    public int VoziloId { get; set; }
    public string Naziv { get; set; } = string.Empty;
    public string TipProblema { get; set; } = string.Empty;
    public DateTime? DatumIsteka { get; set; }
    public int DanaDoIsteka { get; set; }
    public string? Detalji { get; set; }
}
