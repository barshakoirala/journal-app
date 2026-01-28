using JournalAppBlazor.Models;

namespace JournalAppBlazor.Repositories;

/// <summary>
/// Repository interface for Tag with specialized query methods.
/// </summary>
public interface ITagRepository : IRepository<Tag>
{
    /// <summary>
    /// Get all tags accessible to a user (pre-built + user's own), ordered.
    /// </summary>
    Task<List<Tag>> GetAllForUserAsync(int? userId);
    
    /// <summary>
    /// Get only pre-built tags.
    /// </summary>
    Task<List<Tag>> GetPreBuiltAsync();
    
    /// <summary>
    /// Get tag by ID that is accessible to the user.
    /// </summary>
    Task<Tag?> GetByIdForUserAsync(int id, int? userId);
    
    /// <summary>
    /// Get tag by name that is accessible to the user.
    /// </summary>
    Task<Tag?> GetByNameForUserAsync(string name, int? userId);
}
