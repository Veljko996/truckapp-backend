using WebApplication1.Utils.DTOs.PrevoznikDTO;
using WebApplication1.Utils.DTOs.DrzavaDTO;
using ValidationException = WebApplication1.Utils.Exceptions.ValidationException;

namespace WebApplication1.Services.PrevozniciServices;

public class PrevozniciService : IPrevozniciService
{
    private readonly IPrevozniciRepository _repository;

    public PrevozniciService(IPrevozniciRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<PrevoznikDto>> GetAll(int? drzavaId = null)
    {
        IQueryable<Prevoznik> query = drzavaId.HasValue
            ? _repository.GetAllByDrzava(drzavaId.Value)
            : _repository.GetAll();

        var prevoznici = await query.ToListAsync();
        return prevoznici.Select(MapPrevoznikDto).ToList();
    }

    public async Task<PrevoznikDto> GetById(int prevoznikId)
    {
        var prevoznik = await _repository.GetById(prevoznikId);

        if (prevoznik == null)
            throw new NotFoundException("Prevoznik", $"Prevoznik sa ID {prevoznikId} nije pronađen.");

        return MapPrevoznikDto(prevoznik);
    }

    public async Task<PrevoznikDto> Create(CreatePrevoznikDto dto)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(dto.Naziv))
            throw new ValidationException("Naziv", "Naziv prevoznika je obavezan.");

        if (dto.Naziv.Length > 100)
            throw new ValidationException("Naziv", "Naziv prevoznika ne može biti duži od 100 karaktera.");

        // Validate optional fields if provided
        if (!string.IsNullOrWhiteSpace(dto.Kontakt) && dto.Kontakt.Length > 100)
            throw new ValidationException("Kontakt", "Kontakt ne može biti duži od 100 karaktera.");

        if (!string.IsNullOrWhiteSpace(dto.Telefon) && dto.Telefon.Length > 50)
            throw new ValidationException("Telefon", "Telefon ne može biti duži od 50 karaktera.");

        if (!string.IsNullOrWhiteSpace(dto.PIB) && dto.PIB.Length > 20)
            throw new ValidationException("PIB", "PIB ne može biti duži od 20 karaktera.");

        var normalizedDrzavaIds = NormalizeDrzavaIds(dto.DrzavaIds);
        await ValidateDrzavaIdsAsync(normalizedDrzavaIds);

        var prevoznik = dto.Adapt<Prevoznik>();
        prevoznik.DrzaveRada = normalizedDrzavaIds
            .Select(drzavaId => new PrevoznikDrzava
            {
                DrzavaId = drzavaId
            })
            .ToList();

        _repository.Create(prevoznik);

        if (!await _repository.SaveChangesAsync())
            throw new ConflictException("SaveFailed", "Greška prilikom kreiranja prevoznika.");

        var created = await _repository.GetById(prevoznik.PrevoznikId)
            ?? throw new NotFoundException("Prevoznik", $"Prevoznik sa ID {prevoznik.PrevoznikId} nije pronađen.");

        return MapPrevoznikDto(created);
    }

    public async Task Update(int prevoznikId, UpdatePrevoznikDto dto)
    {
        var prevoznik = await _repository.GetById(prevoznikId);

        if (prevoznik == null)
            throw new NotFoundException("PrevoznikNotFound", $"Prevoznik sa ID {prevoznikId} nije pronađen.");

        // Validate if provided
        if (!string.IsNullOrWhiteSpace(dto.Naziv) && dto.Naziv.Length > 100)
            throw new ValidationException("Naziv", "Naziv prevoznika ne može biti duži od 100 karaktera.");

        if (!string.IsNullOrWhiteSpace(dto.Kontakt) && dto.Kontakt.Length > 100)
            throw new ValidationException("Kontakt", "Kontakt ne može biti duži od 100 karaktera.");

        if (!string.IsNullOrWhiteSpace(dto.Telefon) && dto.Telefon.Length > 50)
            throw new ValidationException("Telefon", "Telefon ne može biti duži od 50 karaktera.");

        if (!string.IsNullOrWhiteSpace(dto.PIB) && dto.PIB.Length > 20)
            throw new ValidationException("PIB", "PIB ne može biti duži od 20 karaktera.");

        if (dto.DrzavaIds != null)
        {
            var normalizedDrzavaIds = NormalizeDrzavaIds(dto.DrzavaIds);
            await ValidateDrzavaIdsAsync(normalizedDrzavaIds);
            SyncDrzaveRada(prevoznik, normalizedDrzavaIds);
        }

        dto.Adapt(prevoznik);

        _repository.Update(prevoznik);

        var result = await _repository.SaveChangesAsync();
        if (!result)
            throw new ConflictException("SaveFailed", "Greška prilikom ažuriranja prevoznika.");
    }

    public async Task Delete(int prevoznikId)
    {
        var prevoznik = await _repository.GetById(prevoznikId);
        if (prevoznik == null)
            throw new NotFoundException("PrevoznikNotFound", $"Prevoznik sa ID {prevoznikId} nije pronađen.");

        // TODO: Add business rule checks if needed (e.g., check if prevoznik has active tours)
        // For now, we'll allow deletion
        
        _repository.Delete(prevoznik);

        var result = await _repository.SaveChangesAsync();
        
        if (!result)
            throw new ConflictException("DeleteFailed", "Greška prilikom brisanja prevoznika.");
    }

    private static List<int> NormalizeDrzavaIds(IEnumerable<int>? drzavaIds)
    {
        return (drzavaIds ?? Enumerable.Empty<int>())
            .Where(id => id > 0)
            .Distinct()
            .ToList();
    }

    private async Task ValidateDrzavaIdsAsync(IEnumerable<int> drzavaIds)
    {
        foreach (var drzavaId in drzavaIds)
        {
            if (!await _repository.DrzavaExistsAsync(drzavaId))
                throw new ValidationException("DrzavaIds", $"Država sa ID {drzavaId} ne postoji.");
        }
    }

    private static void SyncDrzaveRada(Prevoznik prevoznik, IReadOnlyCollection<int> requestedDrzavaIds)
    {
        var existing = prevoznik.DrzaveRada.Select(x => x.DrzavaId).ToHashSet();
        var requested = requestedDrzavaIds.ToHashSet();

        var toRemove = prevoznik.DrzaveRada.Where(x => !requested.Contains(x.DrzavaId)).ToList();
        foreach (var item in toRemove)
            prevoznik.DrzaveRada.Remove(item);

        var toAdd = requested.Where(id => !existing.Contains(id));
        foreach (var drzavaId in toAdd)
        {
            prevoznik.DrzaveRada.Add(new PrevoznikDrzava
            {
                DrzavaId = drzavaId
            });
        }
    }

    private static PrevoznikDto MapPrevoznikDto(Prevoznik prevoznik)
    {
        return new PrevoznikDto
        {
            PrevoznikId = prevoznik.PrevoznikId,
            Naziv = prevoznik.Naziv,
            Kontakt = prevoznik.Kontakt,
            Telefon = prevoznik.Telefon,
            PIB = prevoznik.PIB,
            DrzaveRada = prevoznik.DrzaveRada
                .Where(pd => pd.Drzava != null)
                .Select(pd => new PrevoznikDrzavaDto
                {
                    DrzavaId = pd.DrzavaId,
                    Naziv = pd.Drzava!.Naziv,
                    Kod = pd.Drzava.Kod
                })
                .OrderBy(d => d.Naziv)
                .ToList()
        };
    }
}

