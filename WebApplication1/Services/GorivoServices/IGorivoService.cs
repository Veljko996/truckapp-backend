using WebApplication1.Utils.DTOs.GorivoDTO;

namespace WebApplication1.Services.GorivoServices;

public interface IGorivoService
{
    Task<List<GorivoZapisDto>> GetByVoziloIdAsync(int voziloId);
    Task<List<GorivoZapisDto>> GetByNalogIdAsync(int nalogId);
    Task CreateAsync(int voziloId, CreateGorivoZapisDto dto);
    Task DeleteAsync(int gorivoZapisId);
}
