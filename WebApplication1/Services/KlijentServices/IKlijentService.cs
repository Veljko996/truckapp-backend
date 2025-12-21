using WebApplication1.Utils.DTOs.KlijentDTO;

namespace WebApplication1.Services.KlijentServices;

public interface IKlijentService
{
    Task<IEnumerable<KlijentReadDto>> GetAll();
    Task<KlijentReadDto> GetById(int klijentId);
}

