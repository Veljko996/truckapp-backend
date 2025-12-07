using WebApplication1.Utils.DTOs.PrevoznikDTO;

namespace WebApplication1.Services.PrevozniciServices;

public interface IPrevozniciService
{
    Task<IEnumerable<PrevoznikDto>> GetAll();
    Task<PrevoznikDto> GetById(int prevoznikId);
    Task<PrevoznikDto> Create(CreatePrevoznikDto createDto);
    Task<PrevoznikDto> Update(int prevoznikId, UpdatePrevoznikDto updateDto);
    Task Delete(int prevoznikId);
}

