using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DataAccess.Models;

public class Roles
{
    [Key]
    public int RoleId {  get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = new List<User>();
}
