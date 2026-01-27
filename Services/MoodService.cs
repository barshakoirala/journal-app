using Microsoft.EntityFrameworkCore;
using JournalAppBlazor.Data;
using JournalAppBlazor.Models;

namespace JournalAppBlazor.Services;

public interface IMoodService
{
    Task<List<Mood>> GetAllMoodsAsync();
    Task<List<Mood>> GetMoodsByCategoryAsync(MoodCategory category);
    Task<Mood?> GetMoodByIdAsync(int id);
}

public class MoodService : IMoodService
{
    private readonly JournalDbContext _context;

    public MoodService(JournalDbContext context)
    {
        _context = context;
    }

    public async Task<List<Mood>> GetAllMoodsAsync()
    {
        return await _context.Moods
            .OrderBy(m => m.Category)
            .ThenBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<List<Mood>> GetMoodsByCategoryAsync(MoodCategory category)
    {
        return await _context.Moods
            .Where(m => m.Category == category)
            .OrderBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<Mood?> GetMoodByIdAsync(int id)
    {
        return await _context.Moods.FindAsync(id);
    }
}
