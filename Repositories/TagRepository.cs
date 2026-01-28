using Microsoft.EntityFrameworkCore;
using JournalAppBlazor.Data;
using JournalAppBlazor.Models;

namespace JournalAppBlazor.Repositories;

/// <summary>
/// Repository implementation for Tag.
/// </summary>
public class TagRepository : Repository<Tag>, ITagRepository
{
    public TagRepository(JournalDbContext context) : base(context)
    {
    }

    public async Task<List<Tag>> GetAllForUserAsync(int? userId)
    {
        // Return pre-built tags (UserId is null) + user's custom tags
        return await _dbSet
            .Where(t => t.IsPreBuilt || t.UserId == userId)
            .OrderBy(t => t.IsPreBuilt ? 0 : 1)
            .ThenBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<List<Tag>> GetPreBuiltAsync()
    {
        return await _dbSet
            .Where(t => t.IsPreBuilt)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<Tag?> GetByIdForUserAsync(int id, int? userId)
    {
        // Allow access to pre-built tags or user's own tags
        return await _dbSet
            .FirstOrDefaultAsync(t => t.Id == id && (t.IsPreBuilt || t.UserId == userId));
    }

    public async Task<Tag?> GetByNameForUserAsync(string name, int? userId)
    {
        // Check pre-built tags first, then user's custom tags
        return await _dbSet
            .FirstOrDefaultAsync(t =>
                t.Name.ToLower() == name.ToLower() &&
                (t.IsPreBuilt || t.UserId == userId));
    }
}
