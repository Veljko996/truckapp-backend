namespace WebApplication1.Repository.NalogRepository;

public interface INalogRepository
{
    IQueryable<Nalog> GetAll();
    IQueryable<Nalog> GetInterni();
    Task<List<Nalog>> GetNaloziSaIstovaromUKasnjenjuAsync();
    Task<Nalog?> GetByIdAsync(int id);
    Task<string> GetNextNalogBrojAsync();

    void Add(Nalog nalog);
    void Update(Nalog nalog);
    Task<bool> SaveChangesAsync();
}

