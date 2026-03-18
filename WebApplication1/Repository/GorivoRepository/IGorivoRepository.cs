using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.GorivoRepository;

public interface IGorivoRepository
{
    Task<List<GorivoZapis>> GetByVoziloIdAsync(int voziloId);
    Task<List<GorivoZapis>> GetByNalogIdAsync(int nalogId);
    Task<GorivoZapis?> GetByIdAsync(int gorivoZapisId);
    void Add(GorivoZapis entity);
    void Delete(GorivoZapis entity);
    Task<bool> SaveChangesAsync();
}
