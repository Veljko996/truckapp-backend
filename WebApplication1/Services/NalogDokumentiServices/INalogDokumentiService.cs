using WebApplication1.Utils.DTOs.NalogDokumentiDTO;

namespace WebApplication1.Services.NalogDokumentiServices;

public interface INalogDokumentiService
{
    Task<List<NalogDokumentDto>> GetByNalogIdAsync(int nalogId);
    Task<NalogDokumentDto> UploadAsync(int nalogId, UploadNalogDokumentDto dto);
    Task DeleteAsync(int dokumentId);
    Task<(Stream Stream, string ContentType, string FileName)> DownloadAsync(int dokumentId);
    Task<List<TipDokumentaDto>> GetAllTipoviAsync();
}
