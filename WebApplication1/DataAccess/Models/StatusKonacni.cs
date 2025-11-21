using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DataAccess.Models;

public class StatusKonacni
{
    [Key]
    public int StatusId { get; set; }
    public string Naziv { get; set; } = null!;
}
