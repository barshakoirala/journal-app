using JournalAppBlazor.Models;

namespace JournalAppBlazor.Services;

public interface IExportService
{
    Task<string> ExportToHtmlAsync(DateTime startDate, DateTime endDate);
    Task<string> SaveExportToFileAsync(string content, string fileName);
}
