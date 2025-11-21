using WebApplication1.Utils.DTOs.VinjetaDTO;

public class NasaVozilaReadDto
{
    public int VoziloId { get; set; }
    public string Naziv { get; set; } = string.Empty;
    public string? Relacija { get; set; }
    public string? TipVozila { get; set; }
    public string? MarkaModel { get; set; }
    public bool RegistrovanoVozilo { get; set; }
    public DateTime? RegistracijaDatumIsteka { get; set; }
    public DateTime? TehnickiPregledDatumIsteka { get; set; }
    public DateTime? PPAparatDatumIsteka { get; set; }
    public string? Raspolozivost { get; set; }
    
    public List<VinjetaReadDto>? Vinjete { get; set; }
}