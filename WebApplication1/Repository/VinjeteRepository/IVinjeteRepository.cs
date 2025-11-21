using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.VinjeteRepository;

public interface IVinjeteRepository
{
    IQueryable<Vinjeta> GetAll();
    Task<Vinjeta?>GetById(int vinjetaId);
    void Create(Vinjeta vinjeta);
    void Delete(Vinjeta vinjeta);
    void Update(Vinjeta vinjeta);
    Task<bool> SaveChangesAsync();
    Task<bool> VehicleExistsAsync(int voziloId);
    Task<Vinjeta?> GetActiveVignetteForVehicleAsync(int voziloId, string drzavaKod, DateTime datumPocetka, DateTime datumIsteka, int? excludeVinjetaId = null);
}
