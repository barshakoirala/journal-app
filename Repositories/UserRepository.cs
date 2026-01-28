using Microsoft.EntityFrameworkCore;
using JournalAppBlazor.Data;
using JournalAppBlazor.Models;

namespace JournalAppBlazor.Repositories;

/// <summary>
/// Repository implementation for User.
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(JournalDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
    }

    public async Task<bool> HasAnyUsersAsync()
    {
        return await _dbSet.AnyAsync();
    }
}
