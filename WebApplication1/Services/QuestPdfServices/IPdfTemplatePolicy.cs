namespace WebApplication1.Services.QuestPdfServices;

public interface IPdfTemplatePolicy
{
    /// <summary>
    /// Returns allowed template keys for the tenant (lowercase), or empty if none configured.
    /// </summary>
    IReadOnlyList<string> GetAllowedTemplates(int tenantId);

    bool IsAllowed(int tenantId, string templateKey);
}
