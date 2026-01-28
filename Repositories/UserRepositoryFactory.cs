using Microsoft.EntityFrameworkCore;
using JournalAppBlazor.Data;
using JournalAppBlazor.Models;

namespace JournalAppBlazor.Repositories;

/// <summary>
/// Factory-based user repository for use with singleton services.
/// Creates a new DbContext for each operation to avoid scoped lifetime issues.
/// </summary>
public interface IUserRepositoryFactory
{
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> HasAnyUsersAsync();
    Task<User> CreateUserAsync(User user);
    Task UpdateUserAsync(User user);
}

public class UserRepositoryFactory : IUserRepositoryFactory
{
    private readonly IDbContextFactory<JournalDbContext> _contextFactory;

    public UserRepositoryFactory(IDbContextFactory<JournalDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
    }

    public async Task<bool> HasAnyUsersAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Users.AnyAsync();
    }

    public async Task<User> CreateUserAsync(User user)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task UpdateUserAsync(User user)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        context.Users.Update(user);
        await context.SaveChangesAsync();
    }
}
