using WebApplication1.Repository.DrzavaRepository;
using WebApplication1.Utils.DTOs.DrzavaDTO;

namespace WebApplication1.Services.DrzavaServices;

public class DrzavaService : IDrzavaService
{
    private readonly IDrzavaRepository _repository;

    public DrzavaService(IDrzavaRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<DrzavaDto>> GetAllAsync()
    {
        var drzave = await _repository.GetAllActive().ToListAsync();
        return drzave.Select(d => new DrzavaDto
        {
            DrzavaId = d.DrzavaId,
            Naziv = d.Naziv,
            Kod = d.Kod
        });
    }
}
