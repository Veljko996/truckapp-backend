using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.PrevozniciRepository;

public interface IPrevozniciRepository
{
    IQueryable<Prevoznik> GetAll();
    Task<Prevoznik?> GetById(int prevoznikId);
    void Create(Prevoznik prevoznik);
    void Delete(Prevoznik prevoznik);
    void Update(Prevoznik prevoznik);
    Task<bool> SaveChangesAsync();
}

