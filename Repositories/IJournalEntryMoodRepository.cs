using JournalAppBlazor.Models;

namespace JournalAppBlazor.Repositories;

/// <summary>
/// Repository interface for JournalEntryMood (secondary moods junction table).
/// </summary>
public interface IJournalEntryMoodRepository : IRepository<JournalEntryMood>
{
    /// <summary>
    /// Remove all secondary moods for a journal entry.
    /// </summary>
    Task RemoveByJournalEntryIdAsync(int journalEntryId);
    
    /// <summary>
    /// Add multiple secondary moods to a journal entry.
    /// </summary>
    Task AddSecondaryMoodsAsync(int journalEntryId, List<int> moodIds);
}
