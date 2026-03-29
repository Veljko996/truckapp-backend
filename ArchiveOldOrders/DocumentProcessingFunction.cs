using System.Text.Json;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ArchiveOldOrders;

public class DocumentProcessingFunction
{
	private readonly ILogger<DocumentProcessingFunction> _logger;
	private readonly IConfiguration _configuration;

	private static readonly SemaphoreSlim TableEnsureLock = new(1, 1);
	private static bool _resultsTableEnsured;

	public DocumentProcessingFunction(
		ILogger<DocumentProcessingFunction> logger,
		IConfiguration configuration)
	{
		_logger = logger;
		_configuration = configuration;
	}

	[Function(nameof(DocumentProcessingFunction))]
	public async Task Run(
		[QueueTrigger("document-processing", Connection = "AzureWebJobsStorage")]
		string message,
		CancellationToken cancellationToken)
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

			if (payload.TipDokumentaId != 2)
			{
				_logger.LogInformation("Skipping extraction for TipDokumentaId {id}, not a faktura.", payload.TipDokumentaId);
				return;
			}

			_logger.LogInformation("Processing document with DokumentId: {dokumentId}", payload.DokumentId);

			var sqlConnectionString = _configuration["SqlConnectionString"];
			var blobConnectionString = _configuration["AppSettings__Blob"];
			var containerName = _configuration["AppSettings__BlobContainer"];
			var diEndpoint = _configuration["DocumentIntelligence__Endpoint"];
			var diApiKey = _configuration["DocumentIntelligence__ApiKey"];

			if (string.IsNullOrWhiteSpace(sqlConnectionString)
				|| string.IsNullOrWhiteSpace(blobConnectionString)
				|| string.IsNullOrWhiteSpace(diEndpoint)
				|| string.IsNullOrWhiteSpace(diApiKey))
			{
				_logger.LogWarning(
					"Missing configuration (SqlConnectionString, AppSettings__Blob, DocumentIntelligence__Endpoint, or DocumentIntelligence__ApiKey). Skipping message.");
				return;
			}

			if (string.IsNullOrWhiteSpace(containerName))
				containerName = "nalogdokumenti";

			var metadata = await GetDocumentMetadataAsync(sqlConnectionString, payload.DokumentId, cancellationToken);
			if (metadata is null)
			{
				_logger.LogWarning("NalogDokument not found for DokumentId {dokumentId}.", payload.DokumentId);
				return;
			}

			_logger.LogInformation(
				"Loaded document metadata for DokumentId {dokumentId}, content type: {contentType}.",
				payload.DokumentId,
				metadata.ContentType);

			await using var blobStream = await DownloadBlobAsync(
				blobConnectionString,
				containerName,
				metadata.StoredFileName,
				cancellationToken);

			_logger.LogInformation(
				"Downloaded blob for DokumentId {dokumentId}, size: {size} bytes.",
				payload.DokumentId,
				blobStream.Length);

			var keyValues = await AnalyzeDocumentSafeAsync(diEndpoint, diApiKey, blobStream, cancellationToken);
			if (keyValues is null)
				return;

			_logger.LogInformation(
				"Document Intelligence completed for DokumentId {dokumentId}, key-value count: {count}.",
				payload.DokumentId,
				keyValues.Count);

			var extractedJson = JsonSerializer.Serialize(keyValues);

			await EnsureResultsTableAsync(sqlConnectionString, cancellationToken);
			await SaveResultsAsync(sqlConnectionString, payload.DokumentId, extractedJson, isProcessed: true, cancellationToken);

			_logger.LogInformation("Saved Document Intelligence results for DokumentId {dokumentId}.", payload.DokumentId);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error while processing document queue message.");
			throw;
		}
	}

	private async Task<DocumentMetadata?> GetDocumentMetadataAsync(
		string connectionString,
		int dokumentId,
		CancellationToken cancellationToken)
	{
		_logger.LogInformation("SQL: loading NalogDokument row for DokumentId {dokumentId}.", dokumentId);

		await using var conn = new SqlConnection(connectionString);
		await conn.OpenAsync(cancellationToken);

		await using var cmd = conn.CreateCommand();
		cmd.CommandText = """
			SELECT StoredFileName, ContentType
			FROM NalogDokumenti
			WHERE DokumentId = @id AND IsDeleted = 0
			""";
		cmd.Parameters.AddWithValue("@id", dokumentId);

		await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
		if (!await reader.ReadAsync(cancellationToken))
			return null;

		var storedFileName = reader.GetString(0);
		var contentType = reader.GetString(1);
		return new DocumentMetadata(storedFileName, contentType);
	}

	private async Task<MemoryStream> DownloadBlobAsync(
		string connectionString,
		string containerName,
		string blobPath,
		CancellationToken cancellationToken)
	{
		_logger.LogInformation("Blob: downloading from container {containerName}, path length: {pathLength}.", containerName, blobPath.Length);

		var serviceClient = new BlobServiceClient(connectionString);
		var containerClient = serviceClient.GetBlobContainerClient(containerName);
		var blobClient = containerClient.GetBlobClient(blobPath);

		var response = await blobClient.DownloadContentAsync(cancellationToken);
		var bytes = response.Value.Content.ToArray();
		return new MemoryStream(bytes, writable: false);
	}

	private async Task<Dictionary<string, string?>?> AnalyzeDocumentSafeAsync(
		string endpoint,
		string apiKey,
		Stream documentStream,
		CancellationToken cancellationToken)
	{
		_logger.LogInformation("Document Intelligence: starting prebuilt-document analysis.");

		try
		{
			documentStream.Position = 0;
			var client = new DocumentAnalysisClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
			var operation = await client.AnalyzeDocumentAsync(
				WaitUntil.Completed,
				"prebuilt-document",
				documentStream,
				cancellationToken: cancellationToken);

			var result = operation.Value;
			var dict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

			foreach (var kvp in result.KeyValuePairs)
			{
				var key = kvp.Key?.Content?.Trim();
				if (string.IsNullOrEmpty(key))
					continue;
				dict[key] = kvp.Value?.Content;
			}

			_logger.LogInformation("Document Intelligence: analysis finished successfully.");
			return dict;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Document Intelligence analysis failed; message will not be retried for this failure.");
			return null;
		}
	}

	private async Task EnsureResultsTableAsync(string connectionString, CancellationToken cancellationToken)
	{
		await TableEnsureLock.WaitAsync(cancellationToken);
		try
		{
			if (_resultsTableEnsured)
				return;

			_logger.LogInformation("SQL: ensuring DocumentIntelligenceResults table exists.");

			await using var conn = new SqlConnection(connectionString);
			await conn.OpenAsync(cancellationToken);

			await using var cmd = conn.CreateCommand();
			cmd.CommandText = """
				IF NOT EXISTS (
					SELECT 1 FROM sys.tables t
					INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
					WHERE t.name = N'DocumentIntelligenceResults' AND s.name = N'dbo')
				BEGIN
					CREATE TABLE dbo.DocumentIntelligenceResults (
						DokumentId int NOT NULL PRIMARY KEY,
						ExtractedJson nvarchar(max) NULL,
						ProcessedAt datetime2 NOT NULL CONSTRAINT DF_DocumentIntelligenceResults_ProcessedAt DEFAULT SYSUTCDATETIME(),
						IsProcessed bit NOT NULL
					);
				END
				""";

			await cmd.ExecuteNonQueryAsync(cancellationToken);
			_resultsTableEnsured = true;
			_logger.LogInformation("SQL: DocumentIntelligenceResults table is ready.");
		}
		finally
		{
			TableEnsureLock.Release();
		}
	}

	private async Task SaveResultsAsync(
		string connectionString,
		int dokumentId,
		string extractedJson,
		bool isProcessed,
		CancellationToken cancellationToken)
	{
		_logger.LogInformation("SQL: MERGE results for DokumentId {dokumentId}.", dokumentId);

		await using var conn = new SqlConnection(connectionString);
		await conn.OpenAsync(cancellationToken);

		await using var cmd = conn.CreateCommand();
		cmd.CommandText = """
			MERGE dbo.DocumentIntelligenceResults AS target
			USING (SELECT @DokumentId AS DokumentId) AS src
			ON target.DokumentId = src.DokumentId
			WHEN MATCHED THEN
				UPDATE SET
					ExtractedJson = @ExtractedJson,
					ProcessedAt = SYSUTCDATETIME(),
					IsProcessed = @IsProcessed
			WHEN NOT MATCHED THEN
				INSERT (DokumentId, ExtractedJson, ProcessedAt, IsProcessed)
				VALUES (@DokumentId, @ExtractedJson, SYSUTCDATETIME(), @IsProcessed);
			""";

		cmd.Parameters.AddWithValue("@DokumentId", dokumentId);
		cmd.Parameters.AddWithValue("@ExtractedJson", (object?)extractedJson ?? DBNull.Value);
		cmd.Parameters.AddWithValue("@IsProcessed", isProcessed);

		await cmd.ExecuteNonQueryAsync(cancellationToken);
	}

	private sealed record DocumentMetadata(string StoredFileName, string ContentType);
}

public sealed class DocumentProcessingMessage
{
	public int DokumentId { get; set; }

	public int? TipDokumentaId { get; set; }
}
