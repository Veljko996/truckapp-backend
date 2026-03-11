using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace WebApplication1.Services.FileStorage;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobContainerClient _container;

    public AzureBlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration["AppSettings:Blob"]
            ?? throw new InvalidOperationException(
                "Azure Blob connection string 'AppSettings:Blob' is not configured.");

        var containerName = configuration["AppSettings:BlobContainerName"]
            ?? "dokumenti";

        _container = new BlobContainerClient(connectionString, containerName);
        _container.CreateIfNotExists(PublicAccessType.None);
    }

    public async Task<string> SaveAsync(string key, Stream content, string contentType)
    {
        var blob = _container.GetBlobClient(key);

        var headers = new BlobHttpHeaders { ContentType = contentType };
        await blob.UploadAsync(content, new BlobUploadOptions { HttpHeaders = headers });

        return key;
    }

    public async Task<Stream> GetAsync(string key)
    {
        var blob = _container.GetBlobClient(key);

        if (!await blob.ExistsAsync())
            throw new FileNotFoundException($"Blob not found: {key}");

        return await blob.OpenReadAsync();
    }

    public async Task DeleteAsync(string key)
    {
        var blob = _container.GetBlobClient(key);
        await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        var blob = _container.GetBlobClient(key);
        var response = await blob.ExistsAsync();
        return response.Value;
    }
}
