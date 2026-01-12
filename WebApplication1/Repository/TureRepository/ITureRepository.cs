using Microsoft.EntityFrameworkCore.Storage;

public interface ITureRepository
{
    IQueryable<Tura> GetAll();
    Task<Tura?> GetByIdAsync(int id);

    void Add(Tura tura);
    void Update(Tura tura); 
    void Delete(Tura tura);
    Task<bool> SaveChangesAsync();
    Task<string> GetNextTuraBrojAsync();

    // tehničke provere FK vrednosti
    Task<bool> VoziloExistsAsync(int voziloId);
    Task<bool> PrevoznikExistsAsync(int prevoznikId);
    Task<bool> KlijentExistsAsync(int klijentId);
}
