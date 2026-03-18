namespace WebApplication1.Services.QueuePublisher;

//service for publishing messages to the document processing queue
public interface IQueuePublisher
{
    //enqueues a message to the document processing queue
    //nalogDokumentId: the id of the nalog document to process
    //documentType: the type of the document to process
    //cancellationToken: a cancellation token to cancel the operation
    Task EnqueueDocumentProcessingAsync(int DokumentId,  CancellationToken cancellationToken = default);
}