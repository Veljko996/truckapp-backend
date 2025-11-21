using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Utils.DTOs.TuraDTO
{
    public class CreateTuraDto
    {
        [Required(ErrorMessage = "Redni broj je obavezan.")]
        [MaxLength(50, ErrorMessage = "Redni broj ne može biti duži od 50 karaktera.")]
        public string RedniBroj { get; set; } = string.Empty;

        [Required(ErrorMessage = "Relacija je obavezna.")]
        [MaxLength(200, ErrorMessage = "Relacija ne može biti duža od 200 karaktera.")]
        public string Relacija { get; set; } = string.Empty;

        [Required(ErrorMessage = "Datum utovara je obavezan.")]
        public DateTime UtovarDatum { get; set; }

        [Required(ErrorMessage = "Datum istovara je obavezan.")]
        public DateTime IstovarDatum { get; set; }

        [MaxLength(50, ErrorMessage = "Količina robe ne može biti duža od 50 karaktera.")]
        public string? KolicinaRobe { get; set; }

        [MaxLength(50, ErrorMessage = "Težina ne može biti duža od 50 karaktera.")]
        public string? Tezina { get; set; }

        [Required(ErrorMessage = "Opcija prevoza je obavezna.")]
        [MaxLength(20, ErrorMessage = "Opcija prevoza ne može biti duža od 20 karaktera.")]
        public string OpcijaPrevoza { get; set; } = "Solo";

        [MaxLength(100, ErrorMessage = "Vrsta nadogradnje ne može biti duža od 100 karaktera.")]
        public string? VrstaNadogradnje { get; set; }

        [MaxLength(500, ErrorMessage = "Napomena ne može biti duža od 500 karaktera.")]
        public string? Napomena { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Ulazna cena mora biti pozitivan broj.")]
        public decimal? UlaznaCena { get; set; }

        [Required(ErrorMessage = "Prevoznik ID je obavezan.")]
        [Range(1, int.MaxValue, ErrorMessage = "Prevoznik ID mora biti pozitivan broj.")]
        public int PrevoznikId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vozilo ID mora biti pozitivan broj ako je unet.")]
        public int? VoziloId { get; set; } // Optional - can be assigned later
    }

}
