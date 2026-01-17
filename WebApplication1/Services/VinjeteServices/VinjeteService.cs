using ValidationException = WebApplication1.Utils.Exceptions.ValidationException;

namespace WebApplication1.Services.VinjeteServices;

public class VinjeteService : IVinjeteService
{
    private readonly IVinjeteRepository _repository;

    public VinjeteService(IVinjeteRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<VinjetaReadDto>> GetAll()
    {
        var vinjete = await _repository.GetAll().ToListAsync();

        // Don't throw exception if empty - return empty list instead
        // Frontend can handle empty lists better than exceptions
        if (vinjete == null || !vinjete.Any())
            return Enumerable.Empty<VinjetaReadDto>();

        return vinjete.Adapt<IEnumerable<VinjetaReadDto>>();
    }

    public async Task<VinjetaReadDto> GetById(int vinjetaId)
    {
        var vinjeta = await _repository.GetById(vinjetaId);

        if (vinjeta == null)
            throw new NotFoundException("Default", $"Vinjeta sa ID {vinjetaId} nije pronađena.");

        return vinjeta.Adapt<VinjetaReadDto>();
    }

    public async Task<Vinjeta> Create(VinjetaCreateDTO dto)
    {
        // Validate date range
        if (dto.DatumIsteka <= dto.DatumPocetka)
            throw new ValidationException("DatumIsteka", "Datum isteka mora biti posle datuma početka.");

        // Validate vehicle exists if assigned
        if (dto.VoziloId.HasValue)
        {
            var voziloExists = await _repository.VehicleExistsAsync(dto.VoziloId.Value);
            if (!voziloExists)
                throw new NotFoundException("Vozilo", $"Vozilo sa ID {dto.VoziloId.Value} ne postoji.");

            // Check for duplicate vignette (one per vehicle per country) - business rule enforcement
            var existingVinjeta = await _repository.GetActiveVignetteForVehicleAsync(
                dto.VoziloId.Value, 
                dto.DrzavaKod, 
                dto.DatumPocetka, 
                dto.DatumIsteka);
            
            if (existingVinjeta != null)
                throw new ConflictException("VinjetaExists", 
                    $"Već postoji aktivna vinjeta za vozilo {dto.VoziloId.Value} i državu {dto.DrzavaKod} u datom vremenskom periodu. " +
                    $"Postojeća vinjeta: {existingVinjeta.DatumPocetka:yyyy-MM-dd} - {existingVinjeta.DatumIsteka:yyyy-MM-dd}");
        }

        var vinjeta = dto.Adapt<Vinjeta>();
        _repository.Create(vinjeta);

        if (!await _repository.SaveChangesAsync())
            throw new ConflictException("SaveFailed", "Došlo je do greške prilikom čuvanja podataka.");

        return vinjeta;
    }

    public async Task Update(int vinjetaId, VinjetaUpdateDto dto)
    {
        var vinjeta = await _repository.GetById(vinjetaId) ?? throw new NotFoundException("Default", $"Vinjeta sa ID {vinjetaId} nije pronađena.");

        // Validate date range
        if (dto.DatumIsteka <= dto.DatumPocetka)
            throw new ValidationException("DatumIsteka", "Datum isteka mora biti posle datuma početka.");

        // Validate vehicle exists if assigned
        if (dto.VoziloId.HasValue)
        {
            var voziloExists = await _repository.VehicleExistsAsync(dto.VoziloId.Value);
            if (!voziloExists)
                throw new NotFoundException("Vozilo", $"Vozilo sa ID {dto.VoziloId.Value} ne postoji.");

            // Check for duplicate vignette (excluding current one) - business rule enforcement
            var existingVinjeta = await _repository.GetActiveVignetteForVehicleAsync(
                dto.VoziloId.Value, 
                dto.DrzavaKod, 
                dto.DatumPocetka, 
                dto.DatumIsteka,
                vinjetaId);
            
            if (existingVinjeta != null)
                throw new ConflictException("VinjetaExists", 
                    $"Već postoji aktivna vinjeta za vozilo {dto.VoziloId.Value} i državu {dto.DrzavaKod} u datom vremenskom periodu. " +
                    $"Postojeća vinjeta: {existingVinjeta.DatumPocetka:yyyy-MM-dd} - {existingVinjeta.DatumIsteka:yyyy-MM-dd}");
        }

        dto.Adapt(vinjeta);

        if (!await _repository.SaveChangesAsync())
            throw new ConflictException("SaveFailed", "Došlo je do greške prilikom čuvanja podataka.");
    }

    public async Task<bool> Delete(int vinjetaId)
    {
        var vinjeta = await _repository.GetById(vinjetaId);

        if (vinjeta == null)
            throw new NotFoundException("Default", $"Vinjeta sa ID {vinjetaId} nije pronađena.");

        _repository.Delete(vinjeta);

        if (!await _repository.SaveChangesAsync())
            throw new ConflictException("SaveFailed", "Došlo je do greške prilikom čuvanja podataka.");

        return true;
    }
}
