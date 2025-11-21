using WebApplication1.Utils.DTOs.PoslovnicaDTO;

namespace WebApplication1.Services.PoslovnicaServices;

public interface IPoslovnicaService
{
    Task<PoslovnicaReadDto?> GetPoslovnicaByIdAsync(int poslovnicaId);
    Task<IEnumerable<PoslovnicaReadDto>> GetAllPoslovniceAsync();
    Task<PoslovnicaReadDto> CreatePoslovnicaAsync(PoslovnicaCreateDto createDto);
    Task<PoslovnicaReadDto> UpdatePoslovnicaAsync(PoslovnicaUpdateDto updateDto);
    Task<bool> DeletePoslovnicaAsync(int poslovnicaId);
}

