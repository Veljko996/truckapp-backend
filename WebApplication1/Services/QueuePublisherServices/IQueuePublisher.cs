namespace WebApplication1.Services.QueuePublisherServices;

//service for publishing messages to the document processing queue
public interface IQueuePublisher
{
    /// <summary>Enqueues a message with dokument id and tip dokumenta (e.g. faktura = 2 for DI extraction).</summary>
    Task EnqueueDocumentProcessingAsync(int dokumentId, int tipDokumentaId, CancellationToken cancellationToken = default);
}