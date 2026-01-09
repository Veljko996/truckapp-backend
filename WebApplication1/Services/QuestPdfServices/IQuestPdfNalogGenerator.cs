namespace WebApplication1.Services.QuestPdfServices;

public interface IQuestPdfNalogGenerator
{
    Task<byte[]> GeneratePdfAsync(int nalogId, string templateKey);
}

