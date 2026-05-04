namespace WebApplication1.Utils.DTOs.PrevoznikDTO
{
    public class UpdatePrevoznikDto
    {
        public string? Naziv { get; set; }
        public string? Kontakt { get; set; }
        public string? Telefon { get; set; }
        public string? PIB { get; set; }
        public List<int>? DrzavaIds { get; set; }
    }

}
