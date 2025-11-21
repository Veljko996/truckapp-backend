namespace WebApplication1.Services.NasaVozilaServices;

public interface INasaVozilaService
{
    Task<IEnumerable<NasaVozilaReadDto>> GetAll();
    Task<NasaVozilaReadDto> GetById(int voziloId);
    Task<NasaVozilaReadDto> Create(NasaVozilaCreateDto vozilaCreateDto);
    Task<NasaVozilaReadDto?> Update(int voziloId, NasaVozilaUpdateDto nasaVozilaUpdateDto);
    Task<bool> Delete(int voziloId);
}
