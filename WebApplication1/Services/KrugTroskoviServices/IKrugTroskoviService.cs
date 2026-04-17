using WebApplication1.Utils.DTOs.KrugTroskoviDTO;

namespace WebApplication1.Services.KrugTroskoviServices;

public interface IKrugTroskoviService
{
    Task<List<KrugTrosakDto>> GetByKrugIdAsync(int krugId);
    Task CreateAsync(int krugId, CreateKrugTrosakDto dto);
    Task DeleteAsync(int krugTrosakId);
}
