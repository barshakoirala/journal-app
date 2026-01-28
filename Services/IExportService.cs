namespace JournalAppBlazor.Services;

public interface IExportService
{
    Task<string> GenerateHtmlAsync(DateTime startDate, DateTime endDate);
}
