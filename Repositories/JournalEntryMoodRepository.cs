using Microsoft.EntityFrameworkCore;
using JournalAppBlazor.Data;
using JournalAppBlazor.Models;

namespace JournalAppBlazor.Repositories;

/// <summary>
/// Repository implementation for JournalEntryMood.
/// </summary>
public class JournalEntryMoodRepository : Repository<JournalEntryMood>, IJournalEntryMoodRepository
{
    public JournalEntryMoodRepository(JournalDbContext context) : base(context)
    {
    }

    public async Task RemoveByJournalEntryIdAsync(int journalEntryId)
    {
        var moods = await _dbSet.Where(m => m.JournalEntryId == journalEntryId).ToListAsync();
        _dbSet.RemoveRange(moods);
    }

    public async Task AddSecondaryMoodsAsync(int journalEntryId, List<int> moodIds)
    {
        // Add up to 2 secondary moods
        var moodsToAdd = moodIds.Take(2).ToList();
        foreach (var moodId in moodsToAdd)
        {
            await _dbSet.AddAsync(new JournalEntryMood
            {
                JournalEntryId = journalEntryId,
                MoodId = moodId
            });
        }
    }
}
