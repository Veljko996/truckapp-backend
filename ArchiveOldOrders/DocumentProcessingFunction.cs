using System.Text.Json;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ArchiveOldOrders;

public class DocumentProcessingFunction
{
	private readonly ILogger<DocumentProcessingFunction> _logger;

	public DocumentProcessingFunction(ILogger<DocumentProcessingFunction> logger)
	{
		_logger = logger;
	}

	[Function(nameof(DocumentProcessingFunction))]
	public async Task Run([QueueTrigger("document-processing", Connection = "AzureWebJobsStorage")]
		string message)
	{
		_logger.LogInformation("Document processing message received: {messageText}", message);

		try
		{
			var payload = JsonSerializer.Deserialize<DocumentProcessingMessage>(message);

			if (payload is null)
			{
				_logger.LogWarning("Queue message is invalid.");
				return;
			}

			_logger.LogInformation("Processing documents with DokumentId: {dokumentId}", payload.DokumentId);

			await Task.CompletedTask;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error while processing document queue message.");
			throw;
		}
	}
}

public sealed class DocumentProcessingMessage
{
	public int DokumentId { get; set; }
}