using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.DriverAssignmentRepository;

public interface IDriverAssignmentRepository
{
    Task<List<NasaVoziloVozacAssignment>> GetActiveByVoziloIdAsync(int voziloId);
    Task<List<NasaVoziloVozacAssignment>> GetActiveByVoziloIdsAsync(IEnumerable<int> voziloIds);
    Task<List<NasaVoziloVozacAssignment>> GetHistoryByVoziloIdAsync(int voziloId);
    Task<NasaVoziloVozacAssignment?> GetActiveByVoziloSlotAsync(int voziloId, int slotNumber);
    Task<NasaVoziloVozacAssignment?> GetActiveByEmployeeIdAsync(int employeeId);
    Task<Dictionary<int, int>> GetActiveVoziloByEmployeeIdsAsync(IEnumerable<int> employeeIds);
    Task<List<Employee>> GetEligibleDriversAsync();
    void Add(NasaVoziloVozacAssignment assignment);
    Task<bool> SaveChangesAsync();
}

