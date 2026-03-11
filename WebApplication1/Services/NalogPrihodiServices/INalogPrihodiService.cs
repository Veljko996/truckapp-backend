using WebApplication1.Utils.DTOs.NalogPrihodiDTO;

namespace WebApplication1.Services.NalogPrihodiServices;

public interface INalogPrihodiService
{
    Task<List<NalogPrihodDto>> GetByNalogIdAsync(int nalogId);
    Task CreateAsync(int nalogId, CreateNalogPrihodDto dto);
    Task DeleteAsync(int prihodId);
    Task<NalogObracunDto> GetObracunAsync(int nalogId);
    Task<(NalogPrihod? prihod, bool created)> EnsureSeededInitialPrihodAsync(Nalog nalog, Tura tura);
}
