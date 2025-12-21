using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.DataAccess.Models;
public class Nalog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int NalogId { get; set; }
    public string? NalogBroj { get; set; }

    #region polja koja se povlače iz ture i ne menjaju se 
    public int TuraId { get; set; }
    public Tura? Tura { get; set; }
    public string? Relacija { get; set; } // mesto utovara + mesto istovara (iz Ture)
    public DateTime? DatumUtovara { get; set; }
    public DateTime? DatumIstovara { get; set; }
    public string? KolicinaRobe { get; set; }
    #endregion

    #region new business fields
    public string? AdresaUtovara { get; set; }
    public string? VrstaRobe { get; set; }
    public string? Izvoznik { get; set; }
    public string? Spedicija { get; set; }
    public string? GranicniPrelaz { get; set; }
    public string? Uvoznik { get; set; }
    public bool? Istovar { get; set; } = false;
    #endregion

    #region polja koja se vuku iz ture ali mogu se menjati na nalogu
    public string? StatusNaloga { get; set; }
    public int? PrevoznikId { get; set; }
    public Prevoznik? Prevoznik { get; set; }
    public string? IzvoznoCarinjenje { get; set; }
    public string? UvoznoCarinjenje { get; set; }
    #endregion

    #region new notes fields
    public string? NapomenaNalog { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    #endregion
}

