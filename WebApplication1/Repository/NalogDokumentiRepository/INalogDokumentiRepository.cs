using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.NalogDokumentiRepository;

public interface INalogDokumentiRepository
{
    Task<List<NalogDokument>> GetByNalogIdAsync(int nalogId);
    Task<NalogDokument?> GetByIdAsync(int dokumentId);
    void Add(NalogDokument entity);
    void Update(NalogDokument entity);
    Task<bool> SaveChangesAsync();
    Task<List<TipDokumenta>> GetAllTipoviAsync();
}
