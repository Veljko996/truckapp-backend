using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace ArchiveOldOrders;

public class ArchiveOldOrdersFunction
{
	private readonly ILogger _logger;

	public ArchiveOldOrdersFunction(ILoggerFactory loggerFactory)
	{
		_logger = loggerFactory.CreateLogger<ArchiveOldOrdersFunction>();
	}

	// Pokrece se svaki dan u 02:00 (01:00 UTC)
	[Function("ArchiveOldOrders")]
	public async Task Run([TimerTrigger("0 0 1 * * *")] TimerInfo myTimer)
	{
		_logger.LogInformation("Archive job started at: {time}", DateTime.UtcNow);

		string? connectionString = Environment.GetEnvironmentVariable("SqlConnectionString");

		if (string.IsNullOrEmpty(connectionString))
		{
			_logger.LogError("SqlConnectionString is not configured!");
			return;
		}
		try
		{
			using var conn = new SqlConnection(connectionString);

			await conn.OpenAsync();

			var cmd = conn.CreateCommand();
			cmd.CommandText = @"
                UPDATE Nalozi
                SET IsArchived = 1,
                    ArchivedAt = SYSUTCDATETIME()
                WHERE IsArchived = 0
                  AND StatusNaloga = N'Završen'
                  AND DatumIstovara < DATEADD(day, -60, SYSUTCDATETIME());
            ";

			int affectedRows = await cmd.ExecuteNonQueryAsync();

			_logger.LogInformation("Archiving completed. {count} nalog(s) archived.", affectedRows);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error while archiving nalogs.");
		}

		if (myTimer.ScheduleStatus is not null)
		{
			_logger.LogInformation("Next run at: {nextRun}", myTimer.ScheduleStatus.Next);
		}
	}
}
