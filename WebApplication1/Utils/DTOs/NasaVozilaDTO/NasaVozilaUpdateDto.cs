public class NasaVozilaUpdateDto
{
    public string Naziv { get; set; } = string.Empty;
    public string? TipVozila { get; set; }
    public string? MarkaModel { get; set; }
    public bool RegistrovanoVozilo { get; set; }
    public DateTime? RegistracijaDatumIsteka { get; set; }
    public DateTime? TehnickiPregledDatumIsteka { get; set; }
    public DateTime? PPAparatDatumIsteka { get; set; }
    public bool ImaZutuPotvrdu { get; set; }
    public bool ImaBeluPotvrdu { get; set; }
    public DateTime? ZutaPotvrdaVaziDo { get; set; }
    public DateTime? BelaPotvrdaVaziDo { get; set; }
    public int? Kilometraza { get; set; }
    public string? Raspolozivost { get; set; }
    public string? Relacija { get; set; }
}
