using WebApplication1.Utils.DTOs.VrstaNadogradnjeDTO;

namespace WebApplication1.Services.VrstaNadogradnjeServices;

public interface IVrstaNadogradnjeService
{
    Task<IEnumerable<VrstaNadogradnjeReadDto>> GetAll();
    Task<VrstaNadogradnjeReadDto> GetById(int vrstaNadogradnjeId);
}

