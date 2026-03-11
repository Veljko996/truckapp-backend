namespace WebApplication1.Services.FileStorage;

public interface IFileStorageService
{
    Task<string> SaveAsync(string key, Stream content, string contentType);
    Task<Stream> GetAsync(string key);
    Task DeleteAsync(string key);
    Task<bool> ExistsAsync(string key);
}
