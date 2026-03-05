using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.NalogPrihodiRepository;

public interface INalogPrihodiRepository
{
    Task<List<NalogPrihod>> GetByNalogIdAsync(int nalogId);
    Task<NalogPrihod?> GetByIdAsync(int prihodId);
    void Add(NalogPrihod entity);
    void Delete(NalogPrihod entity);
    Task<bool> SaveChangesAsync();
}
