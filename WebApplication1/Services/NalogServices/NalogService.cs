using WebApplication1.Repository.NalogRepository;
using WebApplication1.Utils.DTOs.NalogDTO;

namespace WebApplication1.Services.NalogServices;

public class NalogService : INalogService
{
    private readonly INalogRepository _repository;
    private readonly ITureRepository _turaRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<NalogService> _logger;

    public NalogService(INalogRepository repository, IWebHostEnvironment env, ITureRepository turaRepository, IHttpContextAccessor httpContextAccessor, ILogger<NalogService> logger)
    {
        _repository = repository;
        _turaRepository = turaRepository;
        _httpContextAccessor = httpContextAccessor;
        _env = env;
        _logger = logger;
    }

    public async Task<IEnumerable<NalogReadDto>> GetAllAsync()
    {
        return await _repository.GetAll()
            .Select(n => n.Adapt<NalogReadDto>())
            .ToListAsync();
    }

    public async Task<NalogReadDto?> GetById(int id)
    {
        var nalog = await _repository.GetByIdAsync(id);
        if (nalog == null)
            throw new NotFoundException("Nalog", $"Nalog sa ID {id} nije pronađen.");
        return nalog.Adapt<NalogReadDto>();
    }

    public async Task<NalogReadDto> Create(int turaId, CreateNalogDto dto)
    {
        var tura = await _turaRepository.GetByIdAsync(turaId);
        if (tura == null)
            throw new NotFoundException("Tura", $"Tura sa ID {turaId} nije pronađena.");

        var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

        var nalog = dto.Adapt<Nalog>();

        nalog.TuraId = turaId;
        //nalog.TuraRedniBroj = tura.RedniBroj;
        nalog.Relacija = $"{tura.MestoUtovara} - {tura.MestoIstovara}";
        nalog.DatumUtovara = tura.DatumUtovara;
        nalog.DatumIstovara = tura.DatumIstovara;
        nalog.KolicinaRobe = tura.KolicinaRobe;

        //polja koja se vuku iz ture ali se mogu menjati na nalogu
        nalog.PrevoznikId = tura.PrevoznikId;
        nalog.IzvoznoCarinjenje = tura.IzvoznoCarinjenje;
        nalog.UvoznoCarinjenje = tura.UvoznoCarinjenje;
        nalog.CreatedAt = DateTime.UtcNow;
        nalog.CreatedBy = username;

        _repository.Add(nalog);
        await _repository.SaveChangesAsync();

        nalog.NalogBroj = $"{nalog.NalogId}/{DateTime.Now.Year % 100}";
        nalog.StatusNaloga = "U Toku";
        tura.StatusTure = "Kreiran Nalog";

        await _repository.SaveChangesAsync();

        // Re-query to load navigation properties
        var created = await _repository.GetByIdAsync(nalog.NalogId);
        return created!.Adapt<NalogReadDto>();
    }

    public async Task<NalogReadDto> AssignPrevoznik(int id, AssignPrevoznikDto dto)
    {
        var nalog = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Nalog", $"Nalog sa ID {id} nije pronađen.");

        nalog.PrevoznikId = dto.PrevoznikId;

        _repository.Update(nalog);
        await _repository.SaveChangesAsync();

        // Re-query to load navigation properties
        var updated = await _repository.GetByIdAsync(id);
        return updated!.Adapt<NalogReadDto>();
    }

    public async Task<NalogReadDto> UpdateBusiness(int id, UpdateBusinessFieldsDto dto)
    {
        var nalog = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Nalog", $"Nalog sa ID {id} nije pronađen.");

        dto.Adapt(nalog);
        _repository.Update(nalog);
        await _repository.SaveChangesAsync();

        // Re-query to load navigation properties
        var updated = await _repository.GetByIdAsync(id);
        return updated!.Adapt<NalogReadDto>();
    }
    public async Task<NalogReadDto> UpdateNotes(int id, UpdateNotesDto dto)
    {
        var nalog = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Nalog", $"Nalog sa ID {id} nije pronađen.");

        dto.Adapt(nalog);
        _repository.Update(nalog);
        await _repository.SaveChangesAsync();

        // Re-query to load navigation properties
        var updated = await _repository.GetByIdAsync(id);
        return updated!.Adapt<NalogReadDto>();
    }
    public async Task<NalogReadDto> UpdateStatus(int id, UpdateStatusDto dto)
    {
        var nalog = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Nalog", $"Nalog sa ID {id} nije pronađen.");

        dto.Adapt(nalog);
        _repository.Update(nalog);
        await _repository.SaveChangesAsync();

        // Re-query to load navigation properties
        var updated = await _repository.GetByIdAsync(id);
        return updated!.Adapt<NalogReadDto>();
    }
    public async Task<NalogReadDto> MarkIstovaren(int id, MarkIstovarenDto dto)
    {
        var nalog = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Nalog", $"Nalog sa ID {id} nije pronađen.");

        dto.Adapt(nalog);
        _repository.Update(nalog);
        await _repository.SaveChangesAsync();

        // Re-query to load navigation properties
        var updated = await _repository.GetByIdAsync(id);
        return updated!.Adapt<NalogReadDto>();
    }

    public async Task<NalogReadDto> Storniraj(int id)
    {
        var nalog = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Nalog", $"Nalog sa ID {id} nije pronađen.");

        nalog.StatusNaloga = "Storniran";
        _repository.Update(nalog);
        await _repository.SaveChangesAsync();

        // Re-query to load navigation properties
        var updated = await _repository.GetByIdAsync(id);
        return updated!.Adapt<NalogReadDto>();
    }
    public async Task<NalogReadDto> Ponisti(int id)
    {
        var nalog = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Nalog", $"Nalog sa ID {id} nije pronađen.");

        nalog.StatusNaloga = "Ponisten";
        _repository.Update(nalog);
        await _repository.SaveChangesAsync();

        // Re-query to load navigation properties
        var updated = await _repository.GetByIdAsync(id);
        return updated!.Adapt<NalogReadDto>();
    }
    private static readonly Dictionary<string, string> TemplateMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["mts"] = "MtsNalogTemplate.html",
            ["suins"] = "SuinsNalogTemplate.html",
            ["timnalog"] = "TallTeamNalogTemplate.html"
        };
    public async Task<byte[]> GenerateHtmlAsync(int id, string templateKey)
    {
        var nalog = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(
                "Nalog",
                $"Nalog sa ID {id} nije pronađen."
            );

        if (!TemplateMap.TryGetValue(templateKey, out var templateFile))
            throw new ArgumentException(
                $"Nepoznat template '{templateKey}'.",
                nameof(templateKey)
            );

        var templatePath = Path.Combine(
            _env.ContentRootPath,
            "Templates",
            templateFile
        );

        if (!File.Exists(templatePath))
            throw new FileNotFoundException(
                $"{templateFile} nije pronađen.",
                templatePath
            );

        var html = await File.ReadAllTextAsync(templatePath);

        // Load and embed logo as Base64 (only for suins and mts templates)
        // Embed logo only for suins and mts templates
        if (templateKey.Equals("suins", StringComparison.OrdinalIgnoreCase) ||
            templateKey.Equals("mts", StringComparison.OrdinalIgnoreCase))
        {
            var logoBase64 = await GetLogoBase64Async();

            if (!string.IsNullOrWhiteSpace(logoBase64))
            {
                html = html.Replace("{{LOGO_BASE64}}", logoBase64);
            }
            else
            {
                // Leave empty src → Word will simply not render image
                html = html.Replace("{{LOGO_BASE64}}", string.Empty);
            }
        }
        else
        {
            // Templates without logo
            html = html.Replace("{{LOGO_BASE64}}", string.Empty);
        }


        // Load tura data for additional placeholders
        var tura = nalog.Tura;

        html = html
            // ===== HEADER / OSNOVNO =====
            .Replace("{{NALOG_BROJ}}", nalog.NalogBroj ?? "")
            .Replace(
                "{{DATUM_KREIRANJA}}",
                nalog.CreatedAt.ToString("dd.MM.yyyy")
            )

            // ===== PODACI NALOGA =====
            .Replace("{{RELACIJA}}", nalog.Relacija ?? "")
            .Replace(
                "{{DATUM_UTOVARA}}",
                nalog.DatumUtovara.HasValue
                    ? nalog.DatumUtovara.Value.ToString("dd.MM.yyyy HH:mm")
                    : ""
            )
            .Replace(
                "{{DATUM_ISTOVARA}}",
                nalog.DatumIstovara.HasValue
                    ? nalog.DatumIstovara.Value.ToString("dd.MM.yyyy HH:mm")
                    : ""
            )
            .Replace("{{KOLICINA_ROBE}}", nalog.KolicinaRobe ?? "")
            .Replace("{{VRSTA_ROBE}}", nalog.VrstaRobe ?? "")
            .Replace("{{ADRESA_UTOVARA}}", nalog.AdresaUtovara ?? "")
            .Replace("{{MESTO_UTOVARA}}", tura?.MestoUtovara ?? "")
            .Replace("{{MESTO_ISTOVARA}}", tura?.MestoIstovara ?? "")
            .Replace("{{BROJ_VOZILA}}", tura?.Vozilo?.Naziv ?? "")

            // ===== CARINJENJE / PARTNERI =====
            .Replace("{{IZVOZNIK}}", nalog.Izvoznik ?? "")
            .Replace("{{GRANICNI_PRELAZ}}", nalog.GranicniPrelaz ?? "")
            .Replace("{{UVOZNIK}}", nalog.Uvoznik ?? "")
            .Replace("{{SPEDICIJA}}", nalog.Spedicija ?? "")
            .Replace("{{IZVOZNO_CARINJENJE}}", nalog.IzvoznoCarinjenje ?? "")
            .Replace("{{UVOZNO_CARINJENJE}}", nalog.UvoznoCarinjenje ?? "")

            // ===== FINANSIJE =====
            .Replace("{{CENA_TRANSPORTA}}", tura?.UlaznaCena?.ToString("N2") ?? "")

            // ===== NAPOMENA =====
            .Replace("{{NAPOMENA_NALOGA}}", nalog.NapomenaNalog ?? "");

        return Encoding.UTF8.GetBytes(html);
    }

    private async Task<string> GetLogoBase64Async()
    {
        var logoPath = Path.Combine(
            _env.ContentRootPath,
            "Resources",
            "logo.png"
        );

        Console.WriteLine($"[LOGO] Trying to load logo from: {logoPath}");

        if (!File.Exists(logoPath))
        {
            Console.WriteLine($"[LOGO][ERROR] File NOT FOUND: {logoPath}");
            _logger.LogError("Logo file NOT FOUND at path: {Path}", logoPath);
            throw new FileNotFoundException("Logo file not found", logoPath);
        }

        var logoBytes = await File.ReadAllBytesAsync(logoPath);

        Console.WriteLine($"[LOGO] File loaded. Bytes length: {logoBytes.Length}");

        if (logoBytes.Length == 0)
        {
            Console.WriteLine($"[LOGO][ERROR] File is EMPTY: {logoPath}");
            _logger.LogError("Logo file is EMPTY at path: {Path}", logoPath);
            throw new InvalidOperationException("Logo file is empty");
        }

        var base64 = Convert.ToBase64String(logoBytes);

        Console.WriteLine($"[LOGO] Base64 generated. Length: {base64.Length}");

        if (string.IsNullOrWhiteSpace(base64))
        {
            Console.WriteLine("[LOGO][ERROR] Base64 string is EMPTY after conversion");
            throw new InvalidOperationException("Base64 conversion failed");
        }

        Console.WriteLine("[LOGO] Logo Base64 READY ✔");

        return base64;
    }


}
