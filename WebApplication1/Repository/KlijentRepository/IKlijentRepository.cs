using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.KlijentRepository;

public interface IKlijentRepository
{
    IQueryable<Klijent> GetAll();
    Task<Klijent?> GetById(int klijentId);
}

