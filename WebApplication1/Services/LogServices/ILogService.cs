using WebApplication1.DataAccess.Models;
using WebApplication1.Utils.DTOs.Log;

namespace WebApplication1.Services.LogServices;

public interface ILogService
{
    Task<IEnumerable<Log>> GetAsync();
    Task<bool> CreateAsync(Log log);
}
