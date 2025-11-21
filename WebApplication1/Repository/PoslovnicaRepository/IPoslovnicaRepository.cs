using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.PoslovnicaRepository;

public interface IPoslovnicaRepository
{
    Task<Poslovnica?> GetByIdAsync(int poslovnicaId);
    Task<Poslovnica?> GetByIdWithEmployeesAsync(int poslovnicaId);
    Task<IEnumerable<Poslovnica>> GetAllAsync();
    Task<bool> PJExistsAsync(string pj);
    Task<bool> PJExistsForOtherPoslovnicaAsync(string pj, int excludePoslovnicaId);
    Task AddAsync(Poslovnica poslovnica);
    Task UpdateAsync(Poslovnica poslovnica);
    Task DeleteAsync(Poslovnica poslovnica);
    Task<bool> SaveChangesAsync();
}

