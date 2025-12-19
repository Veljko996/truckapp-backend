namespace WebApplication1.Services.TuraServices;

public interface ITuraService
{
    Task<IEnumerable<TuraReadDto>> GetAll();
    Task<TuraReadDto?> GetById(int id);
    Task<TuraReadDto> Create(CreateTuraDto dto);

    Task<TuraReadDto> UpdateBasic(int id, UpdateTuraDto dto);
    Task<TuraReadDto> UpdateBusiness(int id, UpdateTureBusinessDto dto);
    Task<TuraReadDto> UpdateNotes(int id, UpdateTuraNotesDto dto);
    Task<TuraReadDto> UpdateStatus(int id, UpdateTuraStatusDto dto);

    Task<bool> Delete(int id);
}
