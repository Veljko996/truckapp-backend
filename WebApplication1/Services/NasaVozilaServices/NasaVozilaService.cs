using ValidationException = WebApplication1.Utils.Exceptions.ValidationException;

namespace WebApplication1.Services.NasaVozilaServices;

public class NasaVozilaService : INasaVozilaService
{
    private readonly INasaVozilaRepository _repository;
    private static readonly HashSet<string> ManualOverrideStatuses =
        new(StringComparer.OrdinalIgnoreCase) { "U servisu", "Neaktivno" };

    public NasaVozilaService(INasaVozilaRepository repository)
    {
        _repository = repository;
    }

    private static string? NormalizeManualStatus(string? raspolozivost)
    {
        if (string.IsNullOrWhiteSpace(raspolozivost)) return null;
        return ManualOverrideStatuses.Contains(raspolozivost.Trim()) ? raspolozivost.Trim() : null;
    }

    private static string ComputeEffectiveStatus(string? manualStatus, bool isBusy)
    {
        var manual = NormalizeManualStatus(manualStatus);
        if (manual != null) return manual;
        return isBusy ? "Na turi" : "Slobodno";
    }

    public async Task<IEnumerable<NasaVozilaReadDto>> GetAll()
    {
        // Repository already includes Vinjete and uses AsNoTracking
        var vozila = await _repository.GetAll()
            .ToListAsync();

        var busyIds = await _repository.GetBusyVoziloIdsAsync();
        var dtos = vozila.Adapt<List<NasaVozilaReadDto>>();

        foreach (var dto in dtos)
        {
            dto.Raspolozivost = ComputeEffectiveStatus(dto.Raspolozivost, busyIds.Contains(dto.VoziloId));
        }

        return dtos;
    }

    public async Task<IEnumerable<NasaVozilaReadDto>> GetAvailableForTuraAsync(int? currentTuraId = null)
    {
        var vozila = await _repository.GetAvailableForTuraAsync(currentTuraId);
        return vozila.Adapt<IEnumerable<NasaVozilaReadDto>>();
    }

    public async Task<NasaVozilaReadDto> GetById(int voziloId)
    {
        var vozilo = await _repository.GetById(voziloId);

        if (vozilo == null)
            throw new NotFoundException("Vozilo", $"Vozilo sa ID {voziloId} nije pronađeno.");

        var dto = vozilo.Adapt<NasaVozilaReadDto>();
        var isBusy = await _repository.IsVoziloBusyAsync(voziloId);
        dto.Raspolozivost = ComputeEffectiveStatus(dto.Raspolozivost, isBusy);
        return dto;
    }

    public async Task<NasaVozilaReadDto> Create(NasaVozilaCreateDto dto)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(dto.Naziv))
            throw new ValidationException("Naziv", "Naziv vozila je obavezan.");

        if (dto.Naziv.Length > 100)
            throw new ValidationException("Naziv", "Naziv vozila ne može biti duži od 100 karaktera.");

        // Validate dates if provided
        if (dto.RegistracijaDatumIsteka.HasValue && dto.RegistracijaDatumIsteka.Value < DateTime.UtcNow.AddDays(-1))
            throw new ValidationException("RegistracijaDatumIsteka", 
                "Datum isteka registracije ne može biti više od jednog dana u prošlosti.");

        if (dto.TehnickiPregledDatumIsteka.HasValue && dto.TehnickiPregledDatumIsteka.Value < DateTime.UtcNow.AddDays(-1))
            throw new ValidationException("TehnickiPregledDatumIsteka", 
                "Datum isteka tehničkog pregleda ne može biti više od jednog dana u prošlosti.");

        if (dto.PPAparatDatumIsteka.HasValue && dto.PPAparatDatumIsteka.Value < DateTime.UtcNow.AddDays(-1))
            throw new ValidationException("PPAparatDatumIsteka", 
                "Datum isteka PP aparata ne može biti više od jednog dana u prošlosti.");

        var vozilo = dto.Adapt<NasaVozila>();
        
        // Set default values
        if (string.IsNullOrWhiteSpace(vozilo.Raspolozivost))
            vozilo.Raspolozivost = "Slobodno";
        
        // Auto-update registration status
        vozilo.AzurirajStatusRegistracije();

        _repository.Create(vozilo);

        if (!await _repository.SaveChangesAsync())
            throw new ConflictException("SaveFailed", "Greška prilikom kreiranja vozila.");

        return vozilo.Adapt<NasaVozilaReadDto>();
    }

    public async Task Update(int voziloId, NasaVozilaUpdateDto dto)
    {
        var vozilo = await _repository.GetById(voziloId);

        if (vozilo == null)
            throw new NotFoundException("VoziloNotFound", $"Vozilo sa ID {voziloId} nije pronađeno.");

        // Validate if provided
        if (!string.IsNullOrWhiteSpace(dto.Naziv) && dto.Naziv.Length > 100)
            throw new ValidationException("Naziv", "Naziv vozila ne može biti duži od 100 karaktera.");

        // Validate dates if provided
        if (dto.RegistracijaDatumIsteka.HasValue && dto.RegistracijaDatumIsteka.Value < DateTime.UtcNow.AddDays(-1))
            throw new ValidationException("RegistracijaDatumIsteka", 
                "Datum isteka registracije ne može biti više od jednog dana u prošlosti.");

        if (dto.TehnickiPregledDatumIsteka.HasValue && dto.TehnickiPregledDatumIsteka.Value < DateTime.UtcNow.AddDays(-1))
            throw new ValidationException("TehnickiPregledDatumIsteka", 
                "Datum isteka tehničkog pregleda ne može biti više od jednog dana u prošlosti.");

        if (dto.PPAparatDatumIsteka.HasValue && dto.PPAparatDatumIsteka.Value < DateTime.UtcNow.AddDays(-1))
            throw new ValidationException("PPAparatDatumIsteka", 
                "Datum isteka PP aparata ne može biti više od jednog dana u prošlosti.");

        dto.Adapt(vozilo);

        // Auto-update registration status
        vozilo.AzurirajStatusRegistracije();

        _repository.Update(vozilo);

        var result = await _repository.SaveChangesAsync();
        if (!result)
            throw new ConflictException("SaveFailed", "Greška prilikom ažuriranja vozila.");
    }


    public async Task<bool> Delete(int voziloId)
    {
        var vozilo = await _repository.GetById(voziloId);
        if (vozilo == null)
            throw new NotFoundException("VoziloNotFound", $"Vozilo sa ID {voziloId} nije pronađeno.");

        // Business rule: Check if vehicle has active vignettes
        if (vozilo.HasActiveVignettes())
            throw new ConflictException("VoziloImaVinjete", 
                "Vozilo ne može biti obrisano jer ima aktivne vinjete. Prvo uklonite ili isteknite vinjete.");
        
        _repository.Delete(vozilo);

        var result = await _repository.SaveChangesAsync();
        
        if (!result)
            throw new ConflictException("DeleteFailed", "Greška prilikom brisanja vozila.");
        
        return true;
    }
}
