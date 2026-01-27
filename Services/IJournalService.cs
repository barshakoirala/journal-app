using JournalAppBlazor.Models;

namespace JournalAppBlazor.Services;

public interface IJournalService
{
    Task<JournalEntry?> GetEntryByDateAsync(DateTime date);
    Task<JournalEntry?> GetEntryByIdAsync(int id);
    Task<List<JournalEntry>> GetAllEntriesAsync();
    Task<JournalEntry> CreateEntryAsync(JournalEntry entry, List<int>? secondaryMoodIds = null, List<int>? tagIds = null);
    Task<JournalEntry> UpdateEntryAsync(JournalEntry entry, List<int>? secondaryMoodIds = null, List<int>? tagIds = null);
    Task DeleteEntryAsync(int id);
    Task<bool> EntryExistsForDateAsync(DateTime date);
    
    // Pagination support
    Task<(List<JournalEntry> Entries, int TotalCount)> GetEntriesPaginatedAsync(int pageNumber, int pageSize);
    
    // Search & Filter support
    Task<(List<JournalEntry> Entries, int TotalCount)> SearchEntriesAsync(
        string? searchTerm = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        List<int>? moodIds = null,
        List<int>? tagIds = null,
        int pageNumber = 1,
        int pageSize = 10);
}
