using System.Collections.Concurrent;
using System.Text.Json;
using WebApplication1.Utils.Exceptions;

namespace WebApplication1.Services.ExceptionServices;

/// <summary>
/// Cached exception message service that loads messages once and serves from memory
/// </summary>
public class ExceptionMessageService : IExceptionMessageService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ExceptionMessageService> _logger;
    private static readonly ConcurrentDictionary<string, string> _messageCache = new();
    private static readonly SemaphoreSlim _loadLock = new(1, 1);
    private static bool _isLoaded = false;

    public ExceptionMessageService(
        IWebHostEnvironment env,
        ILogger<ExceptionMessageService> logger)
    {
        _env = env;
        _logger = logger;
        EnsureMessagesLoaded();
    }

    public string? GetMessage(string key, string subKey, string preferredLanguage = "sr-Latn")
    {
        var cacheKey = $"{key}:{subKey}:{preferredLanguage}";
        
        if (_messageCache.TryGetValue(cacheKey, out var cachedMessage))
        {
            return cachedMessage;
        }

        // Try fallback language if preferred not found
        if (preferredLanguage != "en-US")
        {
            var fallbackKey = $"{key}:{subKey}:en-US";
            if (_messageCache.TryGetValue(fallbackKey, out var fallbackMessage))
            {
                return fallbackMessage;
            }
        }

        return null;
    }

    private void EnsureMessagesLoaded()
    {
        if (_isLoaded) return;

        _loadLock.Wait();
        try
        {
            if (_isLoaded) return; // Double-check after acquiring lock

            var path = Path.Combine(_env.ContentRootPath, "Resources", "exceptionMessages.json");
            if (!File.Exists(path))
            {
                _logger.LogWarning("Exception messages file not found at {Path}", path);
                _isLoaded = true;
                return;
            }

            try
            {
                var json = File.ReadAllText(path);
                var messages = JsonSerializer.Deserialize<List<ExceptionMessage>>(json);

                if (messages == null)
                {
                    _logger.LogWarning("Exception messages file is empty or invalid");
                    _isLoaded = true;
                    return;
                }

                // Load all messages into cache
                foreach (var msg in messages)
                {
                    if (msg.Message?.Sr != null)
                    {
                        var srKey = $"{msg.Key}:{msg.SubKey}:sr-Latn";
                        _messageCache[srKey] = msg.Message.Sr;
                    }

                    if (msg.Message?.Eng != null)
                    {
                        var engKey = $"{msg.Key}:{msg.SubKey}:en-US";
                        _messageCache[engKey] = msg.Message.Eng;
                    }
                }

                _logger.LogInformation("Loaded {Count} exception messages into cache", messages.Count);
                _isLoaded = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load exception messages from {Path}", path);
                _isLoaded = true; // Mark as loaded to prevent retry loops
            }
        }
        finally
        {
            _loadLock.Release();
        }
    }
}
