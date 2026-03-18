using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.NasaVozilaRepository;

public interface INasaVozilaRepository
{
    IQueryable<NasaVozila> GetAll();
    Task<NasaVozila?> GetById(int VoziloId);
    void Create(NasaVozila vozilo);
    void Delete(NasaVozila vozilo);
    void Update(NasaVozila vozilo);
    Task<bool> SaveChangesAsync();
    /// <summary>
    /// Vozila koja nisu na aktivnom nalogu (nalog nije Istovaren/Završen/Storniran/Ponisten).
    /// excludeTuraId: pri izmeni ture, njena vozila ne smatraju se zauzetim.
    /// </summary>
    Task<List<NasaVozila>> GetAvailableForTuraAsync(int? excludeTuraId = null);

    /// <summary>
    /// Vozila koja su zauzeta zbog aktivnih naloga (nalog nije Istovaren/Završen/Storniran/Ponisten).
    /// </summary>
    Task<HashSet<int>> GetBusyVoziloIdsAsync();

    /// <summary>
    /// Da li je vozilo zauzeto zbog aktivnog naloga.
    /// </summary>
    Task<bool> IsVoziloBusyAsync(int voziloId);
}
