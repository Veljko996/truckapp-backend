using WebApplication1.Utils.Settings;

namespace WebApplication1.Utils.DTOs.Log;
public class LogCreateDto
{
    public string Process { get; set; } = string.Empty;
    public string Activity { get; set; } = string.Empty;
    public DateTime HappenedAtDate { get; set; } = DateTimeSettings.DateTimeBelgrade();
    public int? Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string? IpAddress { get; set; }
    public string? RequestPath { get; set; }
    public string? RequestMethod { get; set; }
    public bool? HasAccessTokenCookie { get; set; }
    public bool? HasRefreshTokenCookie { get; set; }
}
