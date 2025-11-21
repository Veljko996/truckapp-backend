namespace WebApplication1.Utils.DTOs.PoslovnicaDTO;

public class PoslovnicaReadDto
{
    public int PoslovnicaId { get; set; }
    public string PJ { get; set; } = string.Empty;
    public string Lokacija { get; set; } = string.Empty;
    public string? BrojTelefona { get; set; }
    public string? Email { get; set; }
    public int EmployeeCount { get; set; } // Broj zaposlenih u poslovnici
}

