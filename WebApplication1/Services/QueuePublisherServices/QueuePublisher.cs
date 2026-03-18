using System.Text.Json;
using Azure.Storage.Queues;
using WebApplication1.Utils.DTOs.QueueDTO;

namespace WebApplication1.Services.QueuePublisher;

public sealed class QueuePublisher : IQueuePublisher
{
    private readonly QueueClient _queueClient;

    public QueuePublisher(IConfiguration configuration)
    {
        var connectionString = configuration["AppSettings:Blob"]
            ?? throw new InvalidOperationException("AppSettings:ConnectionString is missing.");

        var queueName = configuration["AppSettings:DocumentProcessingQueueName"]
            ?? throw new InvalidOperationException("AppSettings:DocumentProcessingQueueName is missing.");

        _queueClient = new QueueClient(connectionString, queueName);
    }

    public async Task EnqueueDocumentProcessingAsync(int DokumentId, CancellationToken cancellationToken = default)
    {
        await _queueClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var message = new DocumentProcessingMessage
        {
            DokumentId = DokumentId
        };

        var json = JsonSerializer.Serialize(message);

        await _queueClient.SendMessageAsync(json, cancellationToken);
    }
}