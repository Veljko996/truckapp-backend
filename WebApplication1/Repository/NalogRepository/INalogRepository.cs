namespace WebApplication1.Repository.NalogRepository;

public interface INalogRepository
{
    IQueryable<Nalog> GetAll();
    Task<Nalog?> GetByIdAsync(int id);
   
    void Add(Nalog nalog);
    void Update(Nalog nalog);
    Task<bool> SaveChangesAsync();
}

