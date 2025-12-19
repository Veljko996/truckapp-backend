namespace WebApplication1.Services.NasaVozilaServices;

public class NasaVozilaService : INasaVozilaService
{
    private readonly INasaVozilaRepository _repository;

    public NasaVozilaService(INasaVozilaRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<NasaVozilaReadDto>> GetAll()
    {
        // Repository already includes Vinjete and uses AsNoTracking
        var vozila = await _repository.GetAll()
            .ToListAsync();
        
        return vozila.Adapt<IEnumerable<NasaVozilaReadDto>>();
    }

    public async Task<NasaVozilaReadDto> GetById(int voziloId)
    {
        var vozilo = await _repository.GetById(voziloId);

        if (vozilo == null)
            throw new NotFoundException("Vozilo", $"Vozilo sa ID {voziloId} nije pronađeno.");

        return vozilo.Adapt<NasaVozilaReadDto>();
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

    public async Task<NasaVozilaReadDto> Update(int voziloId, NasaVozilaUpdateDto dto)
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

        return vozilo.Adapt<NasaVozilaReadDto>();
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
