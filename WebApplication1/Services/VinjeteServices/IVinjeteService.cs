namespace WebApplication1.Services.VinjeteServices;

public interface IVinjeteService
{
    Task<IEnumerable<VinjetaReadDto>> GetAll();
    Task<VinjetaReadDto> GetById(int vinjetaId);
    Task<Vinjeta> Create(VinjetaCreateDTO vinjetaCreateDto);
    Task<VinjetaReadDto> Update(int vinjetaId, VinjetaUpdateDto vinjetaUpdateDto);
    Task<bool> Delete(int vinjetaId);
}
