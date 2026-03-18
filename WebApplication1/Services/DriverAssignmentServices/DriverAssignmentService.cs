using WebApplication1.DataAccess.Models;
using WebApplication1.Repository.DriverAssignmentRepository;
using WebApplication1.Repository.NasaVozilaRepository;
using WebApplication1.Utils.DTOs.DriverAssignmentDTO;
using WebApplication1.Utils.Exceptions;

namespace WebApplication1.Services.DriverAssignmentServices;

public class DriverAssignmentService : IDriverAssignmentService
{
    private readonly IDriverAssignmentRepository _repository;
    private readonly INasaVozilaRepository _vozilaRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DriverAssignmentService(
        IDriverAssignmentRepository repository,
        INasaVozilaRepository vozilaRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _vozilaRepository = vozilaRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<DriverAssignmentReadDto>> GetActiveByVoziloIdAsync(int voziloId)
    {
        await EnsureVoziloExistsAsync(voziloId);
        var assignments = await _repository.GetActiveByVoziloIdAsync(voziloId);
        return assignments.Select(MapReadDto).ToList();
    }

    public async Task<List<DriverAssignmentReadDto>> GetActiveByVoziloIdsAsync(IEnumerable<int> voziloIds)
    {
        var ids = voziloIds.Distinct().Where(x => x > 0).ToList();
        if (ids.Count == 0)
            return new List<DriverAssignmentReadDto>();

        var assignments = await _repository.GetActiveByVoziloIdsAsync(ids);
        return assignments.Select(MapReadDto).ToList();
    }

    public async Task<List<DriverAssignmentReadDto>> GetHistoryByVoziloIdAsync(int voziloId)
    {
        await EnsureVoziloExistsAsync(voziloId);
        var assignments = await _repository.GetHistoryByVoziloIdAsync(voziloId);
        return assignments.Select(MapReadDto).ToList();
    }

    public async Task<List<AvailableDriverDto>> GetAvailableDriversAsync(int? voziloId = null)
    {
        var drivers = await _repository.GetEligibleDriversAsync();
        var activeAssignments = await _repository.GetActiveVoziloByEmployeeIdsAsync(drivers.Select(d => d.EmployeeId));
        var result = new List<AvailableDriverDto>();

        foreach (var employee in drivers)
        {
            activeAssignments.TryGetValue(employee.EmployeeId, out var activeVoziloId);
            var hasActiveAssignment = activeVoziloId > 0;
            var isAssignedElsewhere = hasActiveAssignment && (!voziloId.HasValue || activeVoziloId != voziloId.Value);

            result.Add(new AvailableDriverDto
            {
                EmployeeId = employee.EmployeeId,
                FullName = employee.User.FullName,
                EmployeeNumber = employee.EmployeeNumber,
                Position = employee.Position,
                LicenseExpiryDate = employee.LicenseExpiryDate,
                IsAssignedElsewhere = isAssignedElsewhere,
                AssignedVoziloId = hasActiveAssignment ? activeVoziloId : null
            });
        }

        return result
            .OrderBy(x => x.IsAssignedElsewhere)
            .ThenBy(x => x.FullName)
            .ToList();
    }

    public async Task AssignAsync(int voziloId, AssignDriverDto dto)
    {
        await EnsureVoziloExistsAsync(voziloId);
        var now = DateTime.UtcNow;
        var actor = GetActorName();

        var driver = (await _repository.GetEligibleDriversAsync())
            .FirstOrDefault(e => e.EmployeeId == dto.EmployeeId && IsDriverPosition(e.Position))
            ?? throw new NotFoundException("Employee", $"Vozač sa ID {dto.EmployeeId} nije pronađen.");

        var activeForEmployee = await _repository.GetActiveByEmployeeIdAsync(dto.EmployeeId);
        if (activeForEmployee != null && activeForEmployee.VoziloId != voziloId)
            throw new ConflictException("DriverAlreadyAssigned", "Vozač je već dodeljen drugom vozilu.");

        var slotAssignment = await _repository.GetActiveByVoziloSlotAsync(voziloId, dto.SlotNumber);
        if (slotAssignment != null)
        {
            if (slotAssignment.EmployeeId == dto.EmployeeId)
                return;

            slotAssignment.UnassignedAt = now;
            slotAssignment.UnassignedBy = actor;
            if (!string.IsNullOrWhiteSpace(dto.Note))
                slotAssignment.Note = dto.Note.Trim();
        }

        if (activeForEmployee != null && activeForEmployee.VoziloId == voziloId && activeForEmployee.SlotNumber != dto.SlotNumber)
        {
            activeForEmployee.UnassignedAt = now;
            activeForEmployee.UnassignedBy = actor;
        }

        _repository.Add(new NasaVoziloVozacAssignment
        {
            VoziloId = voziloId,
            EmployeeId = dto.EmployeeId,
            SlotNumber = dto.SlotNumber,
            AssignedAt = now,
            AssignedBy = actor,
            Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim()
        });

        await _repository.SaveChangesAsync();
    }

    public async Task UnassignAsync(int voziloId, UnassignDriverDto dto)
    {
        await EnsureVoziloExistsAsync(voziloId);
        var active = await _repository.GetActiveByVoziloSlotAsync(voziloId, dto.SlotNumber)
            ?? throw new NotFoundException("NasaVoziloVozacAssignment", $"Aktivna dodela za slot {dto.SlotNumber} ne postoji.");

        active.UnassignedAt = DateTime.UtcNow;
        active.UnassignedBy = GetActorName();
        if (!string.IsNullOrWhiteSpace(dto.Note))
            active.Note = dto.Note.Trim();

        await _repository.SaveChangesAsync();
    }

    private async Task EnsureVoziloExistsAsync(int voziloId)
    {
        _ = await _vozilaRepository.GetById(voziloId)
            ?? throw new NotFoundException("NasaVozila", $"Vozilo sa ID {voziloId} nije pronađeno.");
    }

    private string? GetActorName()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.Name;
    }

    private static bool IsDriverPosition(string? position)
    {
        if (string.IsNullOrWhiteSpace(position))
            return false;

        var normalized = position.Trim().ToLowerInvariant();
        return normalized.Contains("vozač") || normalized.Contains("vozac");
    }

    private static DriverAssignmentReadDto MapReadDto(NasaVoziloVozacAssignment entity)
    {
        return new DriverAssignmentReadDto
        {
            AssignmentId = entity.AssignmentId,
            VoziloId = entity.VoziloId,
            EmployeeId = entity.EmployeeId,
            SlotNumber = entity.SlotNumber,
            AssignedAt = entity.AssignedAt,
            UnassignedAt = entity.UnassignedAt,
            AssignedBy = entity.AssignedBy,
            UnassignedBy = entity.UnassignedBy,
            Note = entity.Note,
            EmployeeFullName = entity.Employee?.User?.FullName,
            EmployeeNumber = entity.Employee?.EmployeeNumber,
            Position = entity.Employee?.Position,
            LicenseExpiryDate = entity.Employee?.LicenseExpiryDate
        };
    }
}

