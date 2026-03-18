using WebApplication1.Utils.DTOs.DriverAssignmentDTO;

namespace WebApplication1.Services.DriverAssignmentServices;

public interface IDriverAssignmentService
{
    Task<List<DriverAssignmentReadDto>> GetActiveByVoziloIdAsync(int voziloId);
    Task<List<DriverAssignmentReadDto>> GetActiveByVoziloIdsAsync(IEnumerable<int> voziloIds);
    Task<List<DriverAssignmentReadDto>> GetHistoryByVoziloIdAsync(int voziloId);
    Task<List<AvailableDriverDto>> GetAvailableDriversAsync(int? voziloId = null);
    Task AssignAsync(int voziloId, AssignDriverDto dto);
    Task UnassignAsync(int voziloId, UnassignDriverDto dto);
}

