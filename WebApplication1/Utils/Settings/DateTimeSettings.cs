namespace WebApplication1.Utils.Settings;
public static class DateTimeSettings
{
    public static DateTime DateTimeBelgrade()
    {
        TimeZoneInfo belgradeTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

        return TimeZoneInfo.ConvertTime(DateTime.UtcNow, belgradeTimeZone);
    }
}
