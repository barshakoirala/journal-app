using Microsoft.EntityFrameworkCore;
using JournalAppBlazor.Data;
using JournalAppBlazor.Models;

namespace JournalAppBlazor.Repositories;

/// <summary>
/// Repository implementation for Mood.
/// </summary>
public class MoodRepository : Repository<Mood>, IMoodRepository
{
    public MoodRepository(JournalDbContext context) : base(context)
    {
    }

    public async Task<List<Mood>> GetAllOrderedAsync()
    {
        return await _dbSet
            .OrderBy(m => m.Category)
            .ThenBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<List<Mood>> GetByCategoryAsync(MoodCategory category)
    {
        return await _dbSet
            .Where(m => m.Category == category)
            .OrderBy(m => m.Name)
            .ToListAsync();
    }
}
