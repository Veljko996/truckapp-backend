using WebApplication1.Utils.DTOs.TuraDTO;

namespace WebApplication1.Services.TuraServices;

public interface ITuraService
{
    Task<IEnumerable<TuraReadDto>> GetAll();
    Task<TuraReadDto?> GetById(int id);
    Task<TuraReadDto> Create(CreateTuraDto dto);
    Task<TuraReadDto?> Update(int id, UpdateTuraDto dto);
    Task<bool> Delete(int id);
}
