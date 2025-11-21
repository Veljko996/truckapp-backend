using Microsoft.EntityFrameworkCore;
using WebApplication1.DataAccess.Models;
using WebApplication1.Repository.TureRepository;
using WebApplication1.Utils.DTOs.TuraDTO;
using WebApplication1.Utils.Enums;
using WebApplication1.Utils.Exceptions;
using Mapster;

namespace WebApplication1.Services.TuraServices;

public class TuraService : ITuraService
{
    private readonly ITureRepository _repository;

    public TuraService(ITureRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<TuraReadDto>> GetAll()
    {
        var ture = await _repository.GetAll()
            .Select(t => new TuraReadDto
            {
                TuraId = t.TuraId,
                RedniBroj = t.RedniBroj,
                Relacija = t.Relacija,
                UtovarDatum = t.UtovarDatum,
                IstovarDatum = t.IstovarDatum,
                KolicinaRobe = t.KolicinaRobe,
                Tezina = t.Tezina,
                OpcijaPrevoza = t.OpcijaPrevoza,
                VrstaNadogradnje = t.VrstaNadogradnje,
                Napomena = t.Napomena,
                UlaznaCena = t.UlaznaCena,
                StatusTrenutni = t.StatusTrenutni,
                StatusTrenutniVreme = t.StatusTrenutniVreme ?? DateTime.UtcNow,
                StatusKonacni = t.StatusKonacni,
                PrevoznikId = t.PrevoznikId,
                PrevoznikNaziv = t.Prevoznik.Naziv,
                VoziloId = t.VoziloId,
                VoziloNaziv = t.Vozilo != null ? t.Vozilo.Naziv : null
            })
            .ToListAsync();

        return ture;
    }

    public async Task<TuraReadDto?> GetById(int id)
    {
        var tura = await _repository.GetByIdAsync(id);
        if (tura == null)
            throw new NotFoundException("Tura", $"Tura sa zadatim ID {id} nije pronađena.");

        return tura.Adapt<TuraReadDto>();
    }

    public async Task<TuraReadDto> Create(CreateTuraDto dto)
    {
        // 1. Osnovne validacije
        ValidateRequiredFields(dto.RedniBroj, dto.Relacija, dto.PrevoznikId);
        ValidateDates(dto.UtovarDatum, dto.IstovarDatum);

        // 2. Validacija vozila - da li postoji i da li je dostupno
        if (dto.VoziloId.HasValue)
        {
            var voziloExists = await _repository.VoziloExistsAsync(dto.VoziloId.Value);
            if (!voziloExists)
                throw new NotFoundException("Vozilo", $"Vozilo sa ID {dto.VoziloId.Value} ne postoji.");

            // Provera da vozilo nije na aktivnoj turi
            var isAvailable = await _repository.IsVehicleAvailableAsync(dto.VoziloId.Value);
            if (!isAvailable)
                throw new ConflictException("VoziloZauzeto", "Vozilo je trenutno zauzeto i ne može biti dodeljeno novoj turi.");
        }

        // 3. Validacija prevoznika
        var prevoznikExists = await _repository.PrevoznikExistsAsync(dto.PrevoznikId);
        if (!prevoznikExists)
            throw new NotFoundException("Prevoznik", $"Prevoznik sa ID {dto.PrevoznikId} ne postoji.");

        var tura = dto.Adapt<Tura>();
        tura.StatusTrenutni = TuraStatus.UPripremi;
        tura.StatusTrenutniVreme = DateTime.UtcNow;
        tura.StatusKonacni = TuraFinalStatus.UObradi;

        // Transaction za atomičnost
        await using var transaction = await _repository.BeginTransactionAsync();
        try
        {
            // Re-validacija vozila unutar transakcije (za race conditions)
            if (dto.VoziloId.HasValue)
            {
                var isAvailable = await _repository.IsVehicleAvailableAsync(dto.VoziloId.Value);
                if (!isAvailable)
                    throw new ConflictException("VoziloZauzeto", 
                        "Vozilo je trenutno zauzeto i ne može biti dodeljeno novoj turi. Pokušajte ponovo.");
            }

            _repository.Create(tura);
            var result = await _repository.SaveChangesAsync();
            
            if (!result)
                throw new ConflictException("SaveFailed", "Došlo je do greške prilikom čuvanja podataka.");
            
            // Status log
            var statusLog = new TuraStatusLog
            {
                TuraId = tura.TuraId,
                Status = tura.StatusTrenutni,
                Vreme = tura.StatusTrenutniVreme!.Value,
                Napomena = "Tura kreirana"
            };
            await _repository.AddStatusLogAsync(statusLog);
            
            result = await _repository.SaveChangesAsync();
            if (!result)
                throw new ConflictException("StatusLogFailed", "Došlo je do greške prilikom čuvanja status loga.");
            
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
        
        return tura.Adapt<TuraReadDto>();
    }

    public async Task<TuraReadDto> Update(int id, UpdateTuraDto dto)
    {
        var tura = await _repository.GetByIdAsync(id);
        if (tura == null)
            throw new NotFoundException("Tura", $"Tura sa ID {id} nije pronađena.");

        // 1. Osnovne validacije
        ValidateRequiredFields(dto.RedniBroj, dto.Relacija, dto.PrevoznikId);
        ValidateDates(dto.UtovarDatum, dto.IstovarDatum);

        // 2. Normalizacija i validacija statusa
        if (!string.IsNullOrEmpty(dto.StatusTrenutni))
        {
            dto.StatusTrenutni = NormalizeStatus(dto.StatusTrenutni);
            if (!TuraStatus.All.Contains(dto.StatusTrenutni))
                throw new ValidationException("StatusTrenutni", 
                    $"Nevalidan status: '{dto.StatusTrenutni}'. Validni statusi: {string.Join(", ", TuraStatus.All)}");
        }

        // 3. Validacija da završene/otkazane ture ne mogu biti menjane
        if (tura.IsFinalState())
        {
            if (dto.VoziloId != tura.VoziloId)
                throw new ValidationException("VoziloId", 
                    "Vozilo ne može biti promenjeno za završene ili otkazane ture.");

            if (dto.PrevoznikId != tura.PrevoznikId)
                throw new ValidationException("PrevoznikId", 
                    "Prevoznik ne može biti promenjen za završene ili otkazane ture.");

            if (dto.UtovarDatum != tura.UtovarDatum || dto.IstovarDatum != tura.IstovarDatum)
                throw new ValidationException("Datumi", 
                    "Datumi ne mogu biti promenjeni za završene ili otkazane ture.");
        }

        // 4. Validacija vozila - da li postoji i da li je dostupno
        if (dto.VoziloId != tura.VoziloId)
        {
            if (dto.VoziloId.HasValue)
            {
                var voziloExists = await _repository.VoziloExistsAsync(dto.VoziloId.Value);
                if (!voziloExists)
                    throw new NotFoundException("Vozilo", $"Vozilo sa ID {dto.VoziloId.Value} ne postoji.");

                // Provera da vozilo nije na aktivnoj turi (osim trenutne)
                var isAvailable = await _repository.IsVehicleAvailableAsync(dto.VoziloId.Value, id);
                if (!isAvailable)
                    throw new ValidationException("VoziloZauzeto", 
                        "Vozilo je trenutno zauzeto i ne može biti dodeljeno ovoj turi.");
            }
        }

        // 5. Validacija prevoznika
        if (dto.PrevoznikId != tura.PrevoznikId)
        {
            var prevoznikExists = await _repository.PrevoznikExistsAsync(dto.PrevoznikId);
            if (!prevoznikExists)
                throw new NotFoundException("Prevoznik", $"Prevoznik sa ID {dto.PrevoznikId} ne postoji.");
        }

        // 6. Track status change
        var oldStatus = tura.StatusTrenutni;
        var statusChanged = !string.IsNullOrEmpty(dto.StatusTrenutni) && 
                           dto.StatusTrenutni != oldStatus;

        // 7. Map DTO to entity
        dto.Adapt(tura);

        // 8. Auto-update final status based on current status
        if (statusChanged)
        {
            tura.UpdateStatus(tura.StatusTrenutni);
            
            if (tura.StatusTrenutni == TuraStatus.Zavrseno)
                tura.StatusKonacni = TuraFinalStatus.Realizovano;
            else if (tura.StatusTrenutni == TuraStatus.Otkazano)
                tura.StatusKonacni = TuraFinalStatus.NaCekanju;
        }

        // 9. Transaction za atomičnost
        await using var transaction = await _repository.BeginTransactionAsync();
        try
        {
            // Re-validacija vozila unutar transakcije
            if (dto.VoziloId != tura.VoziloId && dto.VoziloId.HasValue)
            {
                var isAvailable = await _repository.IsVehicleAvailableAsync(dto.VoziloId.Value, id);
                if (!isAvailable)
                    throw new ConflictException("VoziloZauzeto", 
                        "Vozilo je trenutno zauzeto i ne može biti dodeljeno ovoj turi. Pokušajte ponovo.");
            }

            _repository.Update(tura);

            // Status log
            if (statusChanged)
            {
                var statusLog = new TuraStatusLog
                {
                    TuraId = tura.TuraId,
                    Status = tura.StatusTrenutni,
                    Vreme = tura.StatusTrenutniVreme!.Value,
                    Napomena = $"Status promenjen sa '{oldStatus}' na '{tura.StatusTrenutni}'"
                };
                await _repository.AddStatusLogAsync(statusLog);
            }

            var result = await _repository.SaveChangesAsync();
            if (!result)
                throw new ConflictException("SaveFailed", "Došlo je do greške prilikom čuvanja podataka.");

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return tura.Adapt<TuraReadDto>();
    }

    /// <summary>
    /// Normalizuje status - konvertuje enum vrednosti (npr. "NaPutu") u string vrednosti (npr. "Na putu")
    /// </summary>
    private static string NormalizeStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return status;

        // Mapiranje enum vrednosti na string vrednosti
        var statusMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "UPripremi", TuraStatus.UPripremi },
            { "UtovarUToku", TuraStatus.UtovarUToku },
            { "UtovarZavrsen", TuraStatus.UtovarZavrsen },
            { "NaPutu", TuraStatus.NaPutu },
            { "Carina", TuraStatus.Carina },
            { "CarinaZavrsena", TuraStatus.CarinaZavrsena },
            { "IstovarUToku", TuraStatus.IstovarUToku },
            { "IstovarZavrsen", TuraStatus.IstovarZavrsen },
            { "Zakasnjenje", TuraStatus.Zakasnjenje },
            { "Zavrseno", TuraStatus.Zavrseno },
            { "Otkazano", TuraStatus.Otkazano }
        };

        // Ako je već validan status, vrati ga
        if (TuraStatus.All.Contains(status))
            return status;

        // Pokušaj mapiranje
        if (statusMap.TryGetValue(status, out var normalizedStatus))
            return normalizedStatus;

        // Ako nije pronađeno, vrati original (validacija će ga odbaciti)
        return status;
    }

    private static void ValidateRequiredFields(string redniBroj, string relacija, int prevoznikId)
    {
        if (string.IsNullOrWhiteSpace(redniBroj))
            throw new ValidationException("RedniBroj", "Redni broj je obavezan.");

        if (string.IsNullOrWhiteSpace(relacija))
            throw new ValidationException("Relacija", "Relacija je obavezna.");

        if (prevoznikId <= 0)
            throw new ValidationException("PrevoznikId", "Prevoznik ID mora biti pozitivan broj.");
    }

    private static void ValidateDates(DateTime utovarDatum, DateTime istovarDatum)
    {
        if (istovarDatum < utovarDatum)
            throw new ValidationException("DatumIstovara", "Datum istovara ne može biti pre datuma utovara.");

        var now = DateTime.UtcNow;
        var maxFutureDate = now.AddYears(2);
        var minPastDate = now.AddMonths(-1);

        if (utovarDatum < minPastDate)
            throw new ValidationException("UtovarDatum", 
                "Datum utovara ne može biti više od mesec dana u prošlosti.");

        if (utovarDatum > maxFutureDate)
            throw new ValidationException("UtovarDatum", 
                "Datum utovara ne može biti više od 2 godine u budućnosti.");

        if (istovarDatum > maxFutureDate)
            throw new ValidationException("IstovarDatum", 
                "Datum istovara ne može biti više od 2 godine u budućnosti.");

        // Trajanje ture ne može biti duže od 180 dana
        var tripDuration = istovarDatum - utovarDatum;
        if (tripDuration.TotalDays > 180)
            throw new ValidationException("TripDuration", 
                "Trajanje ture ne može biti duže od 180 dana.");
    }

    public async Task<bool> Delete(int id)
    {
        var tura = await _repository.GetByIdAsync(id);
        if (tura == null)
            throw new NotFoundException("Tura", $"Tura sa ID {id} nije pronađena.");

        _repository.Delete(tura);
        var result = await _repository.SaveChangesAsync();

        if (!result)
            throw new ConflictException("DeleteFailed", "Greška prilikom brisanja ture.");
        
        return true;
    }
}
