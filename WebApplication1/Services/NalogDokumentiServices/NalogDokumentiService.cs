using Mapster;
using WebApplication1.DataAccess.Models;
using WebApplication1.Repository.NalogDokumentiRepository;
using WebApplication1.Repository.NalogRepository;
using WebApplication1.Services.FileStorage;
using WebApplication1.Utils.DTOs.NalogDokumentiDTO;
using WebApplication1.Utils.Exceptions;
using ValidationException = WebApplication1.Utils.Exceptions.ValidationException;

namespace WebApplication1.Services.NalogDokumentiServices;

public class NalogDokumentiService : INalogDokumentiService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".xls", ".xlsx"
    };

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/jpeg",
        "image/png",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    };

    private readonly INalogDokumentiRepository _repository;
    private readonly INalogRepository _nalogRepository;
    private readonly IFileStorageService _fileStorage;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDocumentProcessingQueuePublisher _queuePublisher;
    private readonly long _maxFileSizeBytes;

    public NalogDokumentiService(
        INalogDokumentiRepository repository,
        INalogRepository nalogRepository,
        IFileStorageService fileStorage,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        IQueuePublisher queuePublisher)
    {
        _repository = repository;
        _nalogRepository = nalogRepository;
        _fileStorage = fileStorage;
        _httpContextAccessor = httpContextAccessor;
        _queuePublisher = queuePublisher;

        var maxMb = configuration.GetValue<int>("FileStorage:MaxFileSizeMB", 25);
        _maxFileSizeBytes = maxMb * 1024L * 1024L;
    }

    public async Task<List<NalogDokumentDto>> GetByNalogIdAsync(int nalogId)
    {
        var nalog = await _nalogRepository.GetByIdAsync(nalogId)
            ?? throw new NotFoundException("Nalog", $"Nalog sa ID {nalogId} nije pronađen.");

        var list = await _repository.GetByNalogIdAsync(nalogId);
        return list.Select(d => d.Adapt<NalogDokumentDto>()).ToList();
    }

    public async Task<NalogDokumentDto> UploadAsync(int nalogId, UploadNalogDokumentDto dto)
    {
        var nalog = await _nalogRepository.GetByIdAsync(nalogId)
            ?? throw new NotFoundException("Nalog", $"Nalog sa ID {nalogId} nije pronađen.");

        var status = nalog.StatusNaloga?.ToLower() ?? "";
        if (status is "storniran" or "ponisten")
            throw new ValidationException("Nalog", "Nije moguće dodavati dokumente na stornirani/poništeni nalog.");

        var file = dto.File;

        if (file.Length == 0)
            throw new ValidationException("Dokument", "Fajl ne može biti prazan.");

        if (file.Length > _maxFileSizeBytes)
            throw new ValidationException("Dokument", $"Fajl je prevelik. Maksimalna veličina je {_maxFileSizeBytes / (1024 * 1024)} MB.");

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
            throw new ValidationException("Dokument", $"Tip fajla '{extension}' nije dozvoljen. Dozvoljeni: {string.Join(", ", AllowedExtensions)}");

        if (!AllowedContentTypes.Contains(file.ContentType))
            throw new ValidationException("Dokument", $"Content type '{file.ContentType}' nije dozvoljen.");

        var tipovi = await _repository.GetAllTipoviAsync();
        if (tipovi.All(t => t.TipDokumentaId != dto.TipDokumentaId))
            throw new ValidationException("Dokument", $"Tip dokumenta sa ID {dto.TipDokumentaId} ne postoji.");

        var storageKey = $"nalog-dokumenti/{nalogId}/{Guid.NewGuid()}{extension}";

        await using var stream = file.OpenReadStream();
        await _fileStorage.SaveAsync(storageKey, stream, file.ContentType);

        var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

        var entity = new NalogDokument
        {
            NalogId = nalogId,
            TipDokumentaId = dto.TipDokumentaId,
            OriginalFileName = SanitizeFileName(file.FileName),
            StoredFileName = storageKey,
            ContentType = file.ContentType,
            FileSize = file.Length,
            Napomena = dto.Napomena?.Trim(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = username
        };

        _repository.Add(entity);
        await _repository.SaveChangesAsync();

        await _queuePublisher.PublishAsync(entity.DokumentId);

        var saved = await _repository.GetByIdAsync(entity.DokumentId);
        return saved!.Adapt<NalogDokumentDto>();
    }

    public async Task DeleteAsync(int dokumentId)
    {
        var dokument = await _repository.GetByIdAsync(dokumentId)
            ?? throw new NotFoundException("NalogDokument", $"Dokument sa ID {dokumentId} nije pronađen.");

        var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

        dokument.IsDeleted = true;
        dokument.DeletedAt = DateTime.UtcNow;
        dokument.DeletedBy = username;

        _repository.Update(dokument);
        await _repository.SaveChangesAsync();
    }

    public async Task<(Stream Stream, string ContentType, string FileName)> DownloadAsync(int dokumentId)
    {
        var dokument = await _repository.GetByIdAsync(dokumentId)
            ?? throw new NotFoundException("NalogDokument", $"Dokument sa ID {dokumentId} nije pronađen.");

        try
        {
            var stream = await _fileStorage.GetAsync(dokument.StoredFileName);
            return (stream, dokument.ContentType, dokument.OriginalFileName);
        }
        catch (FileNotFoundException)
        {
            throw new NotFoundException("NalogDokument", $"Fajl za dokument '{dokument.OriginalFileName}' nije pronađen na disku.");
        }
    }

    public async Task<List<TipDokumentaDto>> GetAllTipoviAsync()
    {
        var tipovi = await _repository.GetAllTipoviAsync();
        return tipovi.Select(t => t.Adapt<TipDokumentaDto>()).ToList();
    }

    private static string SanitizeFileName(string fileName)
    {
        var name = Path.GetFileName(fileName);
        if (name.Length > 500)
            name = name[..500];
        return name;
    }
}
