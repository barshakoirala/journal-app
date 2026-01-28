using JournalAppBlazor.Models;

namespace JournalAppBlazor.Repositories;

/// <summary>
/// Repository interface for JournalEntryTag (junction table).
/// </summary>
public interface IJournalEntryTagRepository : IRepository<JournalEntryTag>
{
    /// <summary>
    /// Remove all tags for a journal entry.
    /// </summary>
    Task RemoveByJournalEntryIdAsync(int journalEntryId);
    
    /// <summary>
    /// Add multiple tags to a journal entry.
    /// </summary>
    Task AddTagsAsync(int journalEntryId, List<int> tagIds);
    
    /// <summary>
    /// Get tag usage statistics for a user.
    /// </summary>
    Task<List<(int TagId, string TagName, int UsageCount)>> GetTagUsageAsync(
        int userId, 
        DateTime? startDate = null, 
        DateTime? endDate = null,
        int? limit = null);
}
