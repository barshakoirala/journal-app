using JournalAppBlazor.Models;

namespace JournalAppBlazor.Repositories;

/// <summary>
/// Repository interface for JournalEntry with specialized query methods.
/// </summary>
public interface IJournalEntryRepository : IRepository<JournalEntry>
{
    /// <summary>
    /// Get entry by ID with all related data (moods, tags) for a specific user.
    /// </summary>
    Task<JournalEntry?> GetByIdWithIncludesAsync(int id, int userId);
    
    /// <summary>
    /// Get entry by date with all related data for a specific user.
    /// </summary>
    Task<JournalEntry?> GetByDateWithIncludesAsync(DateTime date, int userId);
    
    /// <summary>
    /// Get all entries for a user with related data, ordered by date descending.
    /// </summary>
    Task<List<JournalEntry>> GetAllByUserWithIncludesAsync(int userId);
    
    /// <summary>
    /// Get entry by ID with secondary moods and tags (for update operations).
    /// </summary>
    Task<JournalEntry?> GetByIdForUpdateAsync(int id, int userId);
    
    /// <summary>
    /// Check if an entry exists for a specific date and user.
    /// </summary>
    Task<bool> ExistsForDateAsync(DateTime date, int userId);
    
    /// <summary>
    /// Get paginated entries for a user.
    /// </summary>
    Task<(List<JournalEntry> Entries, int TotalCount)> GetPaginatedAsync(int userId, int pageNumber, int pageSize);
    
    /// <summary>
    /// Search entries with filters.
    /// </summary>
    Task<(List<JournalEntry> Entries, int TotalCount)> SearchAsync(
        int userId,
        string? searchTerm = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        List<int>? moodIds = null,
        List<int>? tagIds = null,
        int pageNumber = 1,
        int pageSize = 10);
    
    /// <summary>
    /// Get entry dates for streak calculation.
    /// </summary>
    Task<List<DateTime>> GetEntryDatesAsync(int userId);
    
    /// <summary>
    /// Get entries in date range for a user.
    /// </summary>
    Task<List<JournalEntry>> GetByDateRangeAsync(int userId, DateTime? startDate, DateTime? endDate);
    
    /// <summary>
    /// Get entries with primary mood for analytics.
    /// </summary>
    Task<List<JournalEntry>> GetWithPrimaryMoodAsync(int userId, DateTime? startDate = null, DateTime? endDate = null);
}
