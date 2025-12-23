using WebApplication1.Utils.DTOs.NalogDTO;

namespace WebApplication1.Services.NalogServices;

public interface INalogService
{
    Task<IEnumerable<NalogReadDto>> GetAllAsync();
    Task<NalogReadDto?> GetById(int id);
    Task<NalogReadDto> Create(int turaId, CreateNalogDto dto);
    Task<NalogReadDto> AssignPrevoznik(int id, AssignPrevoznikDto dto);
    Task<NalogReadDto> UpdateBusiness(int id, UpdateBusinessFieldsDto dto);
    Task<NalogReadDto> UpdateNotes(int id, UpdateNotesDto dto);
    Task<NalogReadDto> UpdateStatus(int id, UpdateStatusDto dto);
    Task<NalogReadDto> MarkIstovaren(int id, MarkIstovarenDto dto);
    Task<NalogReadDto> Storniraj(int id);
    Task<NalogReadDto> Ponisti(int id);
    Task<byte[]> GenerateHtmlAsync(int id, string templateKey);
}

