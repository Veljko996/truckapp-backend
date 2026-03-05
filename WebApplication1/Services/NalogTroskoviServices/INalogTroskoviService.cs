using WebApplication1.Utils.DTOs.NalogTroskoviDTO;

namespace WebApplication1.Services.NalogTroskoviServices;

public interface INalogTroskoviService
{
    Task<List<NalogTrosakDto>> GetByNalogIdAsync(int nalogId);
    Task CreateAsync(int nalogId, CreateNalogTrosakDto dto);
    Task DeleteAsync(int trosakId);
    Task<List<TipTroskaDto>> GetAllTipoviAsync();
}
