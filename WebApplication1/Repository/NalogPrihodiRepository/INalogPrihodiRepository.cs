using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.NalogPrihodiRepository;

public interface INalogPrihodiRepository
{
    Task<List<NalogPrihod>> GetByNalogIdAsync(int nalogId);
    Task<NalogPrihod?> GetByIdAsync(int prihodId);
    Task<NalogPrihod?> GetSeededInitialByNalogIdAsync(int nalogId);
    Task<bool> HasAnyByNalogIdAsync(int nalogId);
    void Add(NalogPrihod entity);
    void Update(NalogPrihod entity);
    void Delete(NalogPrihod entity);
    Task<bool> SaveChangesAsync();
}
