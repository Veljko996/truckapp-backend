using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.NalogTroskoviRepository;

public interface INalogTroskoviRepository
{
    Task<List<NalogTrosak>> GetByNalogIdAsync(int nalogId);
    Task<NalogTrosak?> GetByIdAsync(int trosakId);
    void Add(NalogTrosak entity);
    void Delete(NalogTrosak entity);
    Task<bool> SaveChangesAsync();
    Task<List<TipTroska>> GetAllTipoviAsync();
}
