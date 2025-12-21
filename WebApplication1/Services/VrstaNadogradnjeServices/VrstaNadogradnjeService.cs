using WebApplication1.Repository.VrstaNadogradnjeRepository;
using WebApplication1.Utils.DTOs.VrstaNadogradnjeDTO;

namespace WebApplication1.Services.VrstaNadogradnjeServices;

public class VrstaNadogradnjeService : IVrstaNadogradnjeService
{
    private readonly IVrstaNadogradnjeRepository _repository;

    public VrstaNadogradnjeService(IVrstaNadogradnjeRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<VrstaNadogradnjeReadDto>> GetAll()
    {
        var vrsteNadogradnje = await _repository.GetAll()
            .ToListAsync();
        
        // Don't throw exception if empty - return empty list instead
        return vrsteNadogradnje.Adapt<IEnumerable<VrstaNadogradnjeReadDto>>();
    }

    public async Task<VrstaNadogradnjeReadDto> GetById(int vrstaNadogradnjeId)
    {
        var vrstaNadogradnje = await _repository.GetById(vrstaNadogradnjeId);

        if (vrstaNadogradnje == null)
            throw new NotFoundException("VrstaNadogradnje", $"Vrsta nadogradnje sa ID {vrstaNadogradnjeId} nije pronađena.");

        return vrstaNadogradnje.Adapt<VrstaNadogradnjeReadDto>();
    }
}

