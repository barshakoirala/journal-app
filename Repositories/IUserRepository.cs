using JournalAppBlazor.Models;

namespace JournalAppBlazor.Repositories;

/// <summary>
/// Repository interface for User with specialized query methods.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Get user by username (case-insensitive).
    /// </summary>
    Task<User?> GetByUsernameAsync(string username);
    
    /// <summary>
    /// Check if any users exist in the system.
    /// </summary>
    Task<bool> HasAnyUsersAsync();
}
