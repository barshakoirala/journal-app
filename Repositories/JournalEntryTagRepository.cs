using Microsoft.EntityFrameworkCore;
using JournalAppBlazor.Data;
using JournalAppBlazor.Models;

namespace JournalAppBlazor.Repositories;

/// <summary>
/// Repository implementation for JournalEntryTag.
/// </summary>
public class JournalEntryTagRepository : Repository<JournalEntryTag>, IJournalEntryTagRepository
{
    public JournalEntryTagRepository(JournalDbContext context) : base(context)
    {
    }

    public async Task RemoveByJournalEntryIdAsync(int journalEntryId)
    {
        var tags = await _dbSet.Where(t => t.JournalEntryId == journalEntryId).ToListAsync();
        _dbSet.RemoveRange(tags);
    }

    public async Task AddTagsAsync(int journalEntryId, List<int> tagIds)
    {
        foreach (var tagId in tagIds)
        {
            await _dbSet.AddAsync(new JournalEntryTag
            {
                JournalEntryId = journalEntryId,
                TagId = tagId
            });
        }
    }

    public async Task<List<(int TagId, string TagName, int UsageCount)>> GetTagUsageAsync(
        int userId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? limit = null)
    {
        var query = _dbSet
            .Include(t => t.Tag)
            .Include(t => t.JournalEntry)
            .Where(t => t.JournalEntry.UserId == userId)
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(t => t.JournalEntry.Date >= startDate.Value.Date);
        if (endDate.HasValue)
            query = query.Where(t => t.JournalEntry.Date <= endDate.Value.Date);

        var groupedQuery = query
            .GroupBy(t => new { t.TagId, t.Tag.Name })
            .Select(g => new { g.Key.TagId, TagName = g.Key.Name, UsageCount = g.Count() })
            .OrderByDescending(t => t.UsageCount);

        var results = limit.HasValue
            ? await groupedQuery.Take(limit.Value).ToListAsync()
            : await groupedQuery.ToListAsync();

        return results.Select(r => (r.TagId, r.TagName, r.UsageCount)).ToList();
    }
}
