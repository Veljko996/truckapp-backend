namespace WebApplication1.Services.FileStorage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public LocalFileStorageService(IConfiguration configuration)
    {
        var configured = configuration["FileStorage:BasePath"]
            ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");

        _basePath = Path.GetFullPath(configured);
    }

    public async Task<string> SaveAsync(string key, Stream content, string contentType)
    {
        var fullPath = GetSafePath(key);
        var directory = Path.GetDirectoryName(fullPath)!;

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await content.CopyToAsync(fileStream);

        return key;
    }

    public Task<Stream> GetAsync(string key)
    {
        var fullPath = GetSafePath(key);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"File not found: {key}");

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string key)
    {
        var fullPath = GetSafePath(key);

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key)
    {
        var fullPath = GetSafePath(key);
        return Task.FromResult(File.Exists(fullPath));
    }

    private string GetSafePath(string key)
    {
        var sanitized = key.Replace('/', Path.DirectorySeparatorChar)
                           .Replace('\\', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(_basePath, sanitized));

        if (!fullPath.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException($"Path traversal attempt detected for key: {key}");

        return fullPath;
    }
}
