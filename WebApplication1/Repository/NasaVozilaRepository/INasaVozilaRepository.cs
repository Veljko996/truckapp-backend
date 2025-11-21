using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.NasaVozilaRepository;

public interface INasaVozilaRepository
{
    IQueryable<NasaVozila> GetAll();
    Task<NasaVozila?> GetById(int VoziloId);
    void Create(NasaVozila vozilo);
    void Delete(NasaVozila vozilo);
    void Update(NasaVozila vozilo);
    Task<bool> SaveChangesAsync();
}
