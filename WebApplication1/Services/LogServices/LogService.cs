namespace WebApplication1.Services.LogServices;

public class LogService : ILogService
{
    private readonly ILoggingRepository _repository;

    public LogService(ILoggingRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Log>> GetAsync()
    {
        // EF Core async čitanje iz baze
        return await _repository.Get().ToListAsync();
    }

    public async Task<bool> CreateAsync(Log log)
    {
        
        if (log is null)
            throw new ArgumentNullException(nameof(log), "Log objekat ne može biti null.");

        _repository.Create(log);

        return await _repository.SaveChangesAsync();
    }
}