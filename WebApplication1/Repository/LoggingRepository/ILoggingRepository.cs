using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.LoggingRepository
{
    public interface ILoggingRepository
    {
        IQueryable<Log> Get();        
        void Create(Log log);
        Task<bool> SaveChangesAsync();
    }
}
