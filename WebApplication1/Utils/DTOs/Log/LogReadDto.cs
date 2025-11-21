using WebApplication1.Utils.Settings;

namespace WebApplication1.Utils.DTOs.Log;
public class LogReadDto
{
    public string Activity { get; set; } = string.Empty;
    public string Process { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public DateTime HappenedAtDate { get; set; } = DateTimeSettings.DateTimeBelgrade();
    public int? Id { get; set; }
    public string Message { get; set; } = string.Empty;
}
