using WebApplication1.Utils.DTOs.NalogDTO;

namespace WebApplication1.Services.NalogServices;

public interface INalogService
{
    Task<IEnumerable<NalogReadDto>> GetAllAsync(int? vozacUserId = null);
    Task<IEnumerable<NalogReadDto>> GetInterniAsync(int? vozacUserId = null);
    Task<IEnumerable<NalogReadDto>> GetNaloziSaIstovaromUKasnjenjuAsync();
    Task<NalogReadDto?> GetById(int id);
    Task<NalogReadDto> Create(int turaId, CreateNalogDto dto);
    Task<(Nalog nalog, bool created)> EnsureInternalForTuraAsync(Tura tura);
    Task<bool> CancelActiveInternalForTuraAsync(int turaId);
    Task AssignPrevoznik(int id, AssignPrevoznikDto dto);
    Task UpdateBusiness(int id, UpdateBusinessFieldsDto dto);
    Task UpdateNotes(int id, UpdateNotesDto dto);
    Task UpdateStatus(int id, UpdateStatusDto dto);
    Task MarkIstovaren(int id, MarkIstovarenDto dto);
    Task Storniraj(int id);
    Task Ponisti(int id);
}

