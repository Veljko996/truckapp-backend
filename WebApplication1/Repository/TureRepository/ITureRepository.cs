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
    /// <summary>
    /// True if vehicle is assigned to a tura that has a nalog in active status (not Istovaren/Završen/Storniran/Ponisten).
    /// When excludeTuraId is set, that tura's nalog is ignored (for editing that tura).
    /// </summary>
    Task<bool> IsVoziloZauzetoNaNaloguAsync(int voziloId, int? excludeTuraId = null);
}
