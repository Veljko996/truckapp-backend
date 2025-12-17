using Microsoft.EntityFrameworkCore.Storage;

public interface ITureRepository
{
    IQueryable<Tura> GetAll();
    Task<Tura?> GetByIdAsync(int id);

    void Create(Tura tura);
    void Update(Tura tura);
    void Delete(Tura tura);

    Task<bool> SaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();

    Task<bool> VoziloExistsAsync(int voziloId);
    Task<bool> PrevoznikExistsAsync(int prevoznikId);
}
