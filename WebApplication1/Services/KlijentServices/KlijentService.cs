using WebApplication1.Repository.KlijentRepository;
using WebApplication1.Utils.DTOs.KlijentDTO;

namespace WebApplication1.Services.KlijentServices;

public class KlijentService : IKlijentService
{
    private readonly IKlijentRepository _repository;

    public KlijentService(IKlijentRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<KlijentReadDto>> GetAll()
    {
        var klijenti = await _repository.GetAll()
            .ToListAsync();
        
        // Don't throw exception if empty - return empty list instead
        return klijenti.Adapt<IEnumerable<KlijentReadDto>>();
    }

    public async Task<KlijentReadDto> GetById(int klijentId)
    {
        var klijent = await _repository.GetById(klijentId);

        if (klijent == null)
            throw new NotFoundException("Klijent", $"Klijent sa ID {klijentId} nije pronađen.");

        return klijent.Adapt<KlijentReadDto>();
    }
}

