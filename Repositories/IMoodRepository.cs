using JournalAppBlazor.Models;

namespace JournalAppBlazor.Repositories;

/// <summary>
/// Repository interface for Mood with specialized query methods.
/// </summary>
public interface IMoodRepository : IRepository<Mood>
{
    /// <summary>
    /// Get all moods ordered by category and name.
    /// </summary>
    Task<List<Mood>> GetAllOrderedAsync();
    
    /// <summary>
    /// Get moods by category.
    /// </summary>
    Task<List<Mood>> GetByCategoryAsync(MoodCategory category);
}
