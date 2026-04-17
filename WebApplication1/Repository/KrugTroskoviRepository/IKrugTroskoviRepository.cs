using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.KrugTroskoviRepository;

public interface IKrugTroskoviRepository
{
    Task<List<KrugTrosak>> GetByKrugIdAsync(int krugId);
    Task<KrugTrosak?> GetByIdAsync(int krugTrosakId);
    void Add(KrugTrosak entity);
    void Delete(KrugTrosak entity);
    Task<bool> SaveChangesAsync();
}
