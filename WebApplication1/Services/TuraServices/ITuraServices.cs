namespace WebApplication1.Services.TuraServices;

public interface ITuraService
{
    Task<IEnumerable<TuraReadDto>> GetAll();
    Task<TuraReadDto?> GetById(int id);
    Task<TuraReadDto> Create(CreateTuraDto dto);
	Task<TuraReadDto> RecreateAsync(int sourceTuraId);
	Task UpdateBasic(int id, UpdateTuraDto dto);
	Task UpdateBusiness(int id, UpdateTureBusinessDto dto);
	Task UpdateNotes(int id, UpdateTuraNotesDto dto);
	Task UpdateStatus(int id, UpdateTuraStatusDto dto);


	Task<bool> Delete(int id);
}
