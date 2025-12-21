using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.VrstaNadogradnjeRepository;

public interface IVrstaNadogradnjeRepository
{
    IQueryable<VrstaNadogradnje> GetAll();
    Task<VrstaNadogradnje?> GetById(int vrstaNadogradnjeId);
}

