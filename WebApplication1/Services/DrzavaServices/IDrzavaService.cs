using WebApplication1.Utils.DTOs.DrzavaDTO;

namespace WebApplication1.Services.DrzavaServices;

public interface IDrzavaService
{
    Task<IEnumerable<DrzavaDto>> GetAllAsync();
}
