using Azure.Storage.Queues;
using WebApplication1.Utils.DTOs.QueueDTO;

namespace WebApplication1.Services.QueuePublisherServices;

public sealed class QueuePublisher : IQueuePublisher
{
    private readonly QueueClient _queueClient;

    public QueuePublisher(IConfiguration configuration)
    {
        var connectionString = configuration["AppSettings:Blob"]
            ?? throw new InvalidOperationException("AppSettings:ConnectionString is missing.");

        var queueName = configuration["AppSettings:DocumentProcessingQueueName"]
            ?? throw new InvalidOperationException("AppSettings:DocumentProcessingQueueName is missing.");

        //_queueClient = new QueueClient(connectionString, queueName); stari kod
        _queueClient = new QueueClient(connectionString, queueName, new QueueClientOptions()
        {
            MessageEncoding = QueueMessageEncoding.Base64
            });
    }

    public async Task EnqueueDocumentProcessingAsync(int dokumentId, int tipDokumentaId, CancellationToken cancellationToken = default)
    {
        await _queueClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var message = new DocumentProcessingMessage
        {
            DokumentId = dokumentId,
            TipDokumentaId = tipDokumentaId
        };

        var json = JsonSerializer.Serialize(message);

        await _queueClient.SendMessageAsync(json, cancellationToken);
    }
}