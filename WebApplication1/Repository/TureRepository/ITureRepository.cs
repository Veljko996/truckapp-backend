using Microsoft.EntityFrameworkCore.Storage;

namespace WebApplication1.Repository.TureRepository;

public interface ITureRepository
{
    IQueryable<Tura> GetAll();
    Task<Tura?> GetByIdAsync(int id);
    void Create(Tura tura);
    void Update(Tura tura);
    void Delete(Tura tura);
    Task<bool> SaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task<bool> IsVehicleAvailableAsync(int voziloId, int? excludeTuraId = null);
    Task<bool> PrevoznikExistsAsync(int prevoznikId);
    Task<bool> VoziloExistsAsync(int voziloId);
    Task AddStatusLogAsync(TuraStatusLog statusLog);
}
