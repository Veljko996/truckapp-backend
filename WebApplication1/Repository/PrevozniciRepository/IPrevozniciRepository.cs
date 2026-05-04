using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.PrevozniciRepository;

public interface IPrevozniciRepository
{
    IQueryable<Prevoznik> GetAll();
    IQueryable<Prevoznik> GetAllByDrzava(int drzavaId);
    Task<Prevoznik?> GetById(int prevoznikId);
    Task<bool> DrzavaExistsAsync(int drzavaId);
    void Create(Prevoznik prevoznik);
    void Delete(Prevoznik prevoznik);
    void Update(Prevoznik prevoznik);
    Task<bool> SaveChangesAsync();
}

