namespace WebApplication1.Services.NalogVozacAccessServices;

public interface INalogVozacAccessService
{
    IQueryable<Nalog> ApplyVozacFilter(IQueryable<Nalog> query, int userId);
    Task<bool> CanAccessNalogAsync(int userId, int nalogId, CancellationToken ct = default);
    Task<bool> CanAccessVoziloAsync(int userId, int voziloId, CancellationToken ct = default);
}
