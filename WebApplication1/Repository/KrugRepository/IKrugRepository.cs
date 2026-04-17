using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.KrugRepository;

public interface IKrugRepository
{
    IQueryable<Krug> GetAll();
    Task<Krug?> GetByIdAsync(int id);
    Task<Krug?> GetByIdWithTureAsync(int id);
    Task<Krug?> GetOpenByVoziloIdAsync(int voziloId);
    Task<string> GetNextKrugBrojAsync();

    void Add(Krug entity);
    void Update(Krug entity);
    void Delete(Krug entity);

    Task<bool> SaveChangesAsync();
}
