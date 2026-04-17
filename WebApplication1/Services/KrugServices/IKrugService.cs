using WebApplication1.Utils.DTOs.KrugDTO;
using WebApplication1.Utils.DTOs.NalogDTO;

namespace WebApplication1.Services.KrugServices;

public interface IKrugService
{
    Task<List<KrugReadDto>> GetAllAsync(int? vozacUserId = null);
    Task<KrugDetailsDto> GetDetailsAsync(int krugId, int? vozacUserId = null);

    /// <summary>
    /// Server-side agregirani finansijski rezime kruga (Faza 5).
    /// Vraća isti sadržaj kao "Rezime" tab u KrugDetails, ali bez učitavanja celog DTO-a.
    /// </summary>
    Task<KrugFinancialSummaryDto> GetFinancialSummaryAsync(int krugId, int? vozacUserId = null);

    /// <summary>
    /// Smart suggest: vraća OTVOREN krug za vozilo (ako postoji). Koristi se u UI flow-u
    /// "Kreiraj krug iz naloga" / "Dodaj postojeći nalog" da ne pukne na 409.
    /// </summary>
    Task<KrugReadDto?> GetOpenByVoziloAsync(int voziloId, int? vozacUserId = null);

    Task<KrugReadDto> CreateAsync(CreateKrugDto dto);

    /// <summary>
    /// Kreira Krug iz postojećeg Naloga: vozilo se uzima iz Tura tog Naloga
    /// i ta Tura se odmah povezuje sa Krugom.
    /// </summary>
    Task<KrugReadDto> CreateFromNalogAsync(int nalogId);

    /// <summary>
    /// Unified command: kreira novu Tura, postavi joj KrugId, i ensure-uj
    /// interni Nalog za nju koristeći postojeću logiku iz NalogService-a.
    /// </summary>
    Task<NalogReadDto> CreateNalogForKrugAsync(int krugId, CreateNalogForKrugDto dto);

    Task CloseAsync(int krugId);
    Task DeleteAsync(int krugId);
}
