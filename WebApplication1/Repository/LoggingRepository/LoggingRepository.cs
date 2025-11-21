using Microsoft.EntityFrameworkCore;
using WebApplication1.DataAccess;
using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.LoggingRepository;

public class LoggingRepository : ILoggingRepository
{
    private readonly ILogger<LoggingRepository> _logger;
    private readonly TruckContext _context;

    public LoggingRepository(ILogger<LoggingRepository> logger, TruckContext context)
    {
        _logger = logger;
        _context = context;
    }
    public IQueryable<Log> Get()
        => _context.Logs.AsNoTracking().OrderByDescending(l => l.HappenedAtDate);

    public void Create(Log log)
    {
        if (log != null)
            _context.Logs.Add(log);
        else
            throw new ArgumentNullException(nameof(log));
    }

    public async Task<bool> SaveChangesAsync()
    {
        var affected = await _context.SaveChangesAsync();
        if (affected == 0)
            _logger.LogWarning("Nije bilo promena prilikom čuvanja logova.");
        return affected > 0;
    }


}
