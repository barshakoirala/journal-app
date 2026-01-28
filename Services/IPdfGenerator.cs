namespace JournalAppBlazor.Services;

public interface IPdfGenerator
{
    Task<byte[]> GeneratePdfFromHtmlAsync(string html);
}
