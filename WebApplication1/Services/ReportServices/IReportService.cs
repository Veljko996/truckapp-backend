using WebApplication1.Utils.DTOs.ReportDTO;

namespace WebApplication1.Services.ReportServices;

public interface IReportService
{
    /// <summary>
    /// Izveštaj o zatvorenim krugovima u [from, to] (po datumu zatvaranja kruga).
    /// Vraća prikaz po krugu + zbirno po vozilu + ukupno, sve po valuti.
    /// </summary>
    Task<KrugoviReportDto> GetKrugoviReportAsync(DateTime from, DateTime to, int? voziloId);
}
