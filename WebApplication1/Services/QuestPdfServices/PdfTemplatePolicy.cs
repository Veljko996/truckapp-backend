namespace WebApplication1.Services.QuestPdfServices;

/// <summary>
/// Maps tenant IDs to allowed PDF template query keys. Configure under PdfTemplates:AllowedByTenant in appsettings;
/// if the section is empty, built-in defaults apply (tenant 1 = mts/suins/timnalog, tenant 2 = fetico).
/// </summary>
public class PdfTemplatePolicy : IPdfTemplatePolicy
{
    private readonly Dictionary<int, HashSet<string>> _map;

    public PdfTemplatePolicy(IConfiguration configuration)
    {
        _map = new Dictionary<int, HashSet<string>>();
        var section = configuration.GetSection("PdfTemplates:AllowedByTenant");

        foreach (var child in section.GetChildren())
        {
            if (!int.TryParse(child.Key, out var tenantId))
                continue;

            var keys = child.Get<string[]>();
            if (keys is { Length: > 0 })
                _map[tenantId] = new HashSet<string>(keys.Select(Normalize), StringComparer.OrdinalIgnoreCase);
        }

        EnsureTenantDefaultsIfMissing();
    }

    /// <summary>
    /// Suins (1) i Fetico (2) dobijaju podrazumevane šablone ako u konfiguraciji nisu eksplicitno postavljeni.
    /// </summary>
    private void EnsureTenantDefaultsIfMissing()
    {
        if (!_map.ContainsKey(1))
            _map[1] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "mts", "suins", "timnalog" };
        if (!_map.ContainsKey(2))
            _map[2] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "fetico" };
    }

    public IReadOnlyList<string> GetAllowedTemplates(int tenantId)
    {
        return _map.TryGetValue(tenantId, out var set)
            ? set.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList()
            : Array.Empty<string>();
    }

    public bool IsAllowed(int tenantId, string templateKey)
    {
        var key = Normalize(templateKey);
        return _map.TryGetValue(tenantId, out var set) && set.Contains(key);
    }

    private static string Normalize(string templateKey) =>
        templateKey.Trim().ToLowerInvariant();
}
