namespace WebApplication1.Utils.DTOs.NalogDTO;

public class NalogReadDto
{
    public int NalogId { get; set; }
    public string? NalogBroj { get; set; }
    
    public int TuraId { get; set; }
    public string? Relacija { get; set; }
    public DateTime? DatumUtovara { get; set; }
    public DateTime? DatumIstovara { get; set; }
    public string? KolicinaRobe { get; set; }
    
    public string? AdresaUtovara { get; set; }
    public string? VrstaRobe { get; set; }
    public string? Izvoznik { get; set; }
    public string? Spedicija { get; set; }
    public string? GranicniPrelaz { get; set; }
    public string? Uvoznik { get; set; }
    public bool? Istovar { get; set; }
    
    public string? StatusNaloga { get; set; }
    public int? PrevoznikId { get; set; }
    public string? IzvoznoCarinjenje { get; set; }
    public string? UvoznoCarinjenje { get; set; }
    
    public string? NapomenaNalog { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    
    // Lookup names
    public string? PrevoznikNaziv { get; set; }
}

