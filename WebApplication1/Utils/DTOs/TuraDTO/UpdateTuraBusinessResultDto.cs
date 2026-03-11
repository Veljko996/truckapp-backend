namespace WebApplication1.Utils.DTOs.TuraDTO;

public class UpdateTuraBusinessResultDto
{
    public int TuraId { get; set; }
    public bool IsInternalAssignment { get; set; }
    public int? NalogId { get; set; }
    public bool NalogCreatedNow { get; set; }
    public bool SeededPrihodCreatedNow { get; set; }
}
