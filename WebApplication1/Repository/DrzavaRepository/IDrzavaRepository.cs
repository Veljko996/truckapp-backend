using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.DrzavaRepository;

public interface IDrzavaRepository
{
    IQueryable<Drzava> GetAllActive();
    Task<Drzava?> GetByIdAsync(int drzavaId);
}
