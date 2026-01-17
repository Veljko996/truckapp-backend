namespace WebApplication1.Services.ExceptionServices;

/// <summary>
/// Service for retrieving localized exception messages from cached resources
/// </summary>
public interface IExceptionMessageService
{
    /// <summary>
    /// Gets a localized exception message by key and subkey
    /// </summary>
    /// <param name="key">Exception type key (e.g., "NotFoundException")</param>
    /// <param name="subKey">Specific sub-key for the exception (e.g., "Tura")</param>
    /// <param name="preferredLanguage">Preferred language code (default: "sr-Latn")</param>
    /// <returns>Localized message or null if not found</returns>
    string? GetMessage(string key, string subKey, string preferredLanguage = "sr-Latn");
}
