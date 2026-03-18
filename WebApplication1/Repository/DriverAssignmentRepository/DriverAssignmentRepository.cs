using Microsoft.EntityFrameworkCore;
using WebApplication1.DataAccess;
using WebApplication1.DataAccess.Models;

namespace WebApplication1.Repository.DriverAssignmentRepository;

public class DriverAssignmentRepository : IDriverAssignmentRepository
{
    private readonly TruckContext _context;

    public DriverAssignmentRepository(TruckContext context)
    {
        _context = context;
    }

    public async Task<List<NasaVoziloVozacAssignment>> GetActiveByVoziloIdAsync(int voziloId)
    {
        return await _context.NasaVoziloVozacAssignments
            .Include(a => a.Employee)
            .ThenInclude(e => e!.User)
            .Where(a => a.VoziloId == voziloId && a.UnassignedAt == null)
            .OrderBy(a => a.SlotNumber)
            .ToListAsync();
    }

    public async Task<List<NasaVoziloVozacAssignment>> GetActiveByVoziloIdsAsync(IEnumerable<int> voziloIds)
    {
        var ids = voziloIds.Distinct().ToList();
        if (ids.Count == 0)
            return new List<NasaVoziloVozacAssignment>();

        return await _context.NasaVoziloVozacAssignments
            .Include(a => a.Employee)
            .ThenInclude(e => e!.User)
            .Where(a => a.UnassignedAt == null && ids.Contains(a.VoziloId))
            .OrderBy(a => a.VoziloId)
            .ThenBy(a => a.SlotNumber)
            .ToListAsync();
    }

    public async Task<List<NasaVoziloVozacAssignment>> GetHistoryByVoziloIdAsync(int voziloId)
    {
        return await _context.NasaVoziloVozacAssignments
            .Include(a => a.Employee)
            .ThenInclude(e => e!.User)
            .Where(a => a.VoziloId == voziloId)
            .OrderByDescending(a => a.AssignedAt)
            .ToListAsync();
    }

    public Task<NasaVoziloVozacAssignment?> GetActiveByVoziloSlotAsync(int voziloId, int slotNumber)
    {
        return _context.NasaVoziloVozacAssignments
            .Include(a => a.Employee)
            .ThenInclude(e => e!.User)
            .FirstOrDefaultAsync(a =>
                a.VoziloId == voziloId &&
                a.SlotNumber == slotNumber &&
                a.UnassignedAt == null);
    }

    public Task<NasaVoziloVozacAssignment?> GetActiveByEmployeeIdAsync(int employeeId)
    {
        return _context.NasaVoziloVozacAssignments
            .Include(a => a.Employee)
            .ThenInclude(e => e!.User)
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.UnassignedAt == null);
    }

    public async Task<Dictionary<int, int>> GetActiveVoziloByEmployeeIdsAsync(IEnumerable<int> employeeIds)
    {
        var ids = employeeIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<int, int>();

        return await _context.NasaVoziloVozacAssignments
            .Where(a => a.UnassignedAt == null && ids.Contains(a.EmployeeId))
            .ToDictionaryAsync(a => a.EmployeeId, a => a.VoziloId);
    }

    public async Task<List<Employee>> GetEligibleDriversAsync()
    {
        return await _context.Employees
            .Include(e => e.User)
            .Where(e =>
                e.IsActive &&
                e.User.IsActive &&
                e.Position != null &&
                (e.Position.ToLower().Contains("vozač") || e.Position.ToLower().Contains("vozac")))
            .OrderBy(e => e.User.FullName)
            .ToListAsync();
    }

    public void Add(NasaVoziloVozacAssignment assignment)
    {
        _context.NasaVoziloVozacAssignments.Add(assignment);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}

