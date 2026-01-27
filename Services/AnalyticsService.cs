using Microsoft.EntityFrameworkCore;
using JournalAppBlazor.Data;
using JournalAppBlazor.Models;

namespace JournalAppBlazor.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly JournalDbContext _context;

    public AnalyticsService(JournalDbContext context)
    {
        _context = context;
    }

    public async Task<MoodDistribution> GetMoodDistributionAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.JournalEntries
            .Include(e => e.PrimaryMood)
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(e => e.Date >= startDate.Value.Date);
        if (endDate.HasValue)
            query = query.Where(e => e.Date <= endDate.Value.Date);

        var entries = await query.ToListAsync();

        var positiveCount = entries.Count(e => e.PrimaryMood.Category == MoodCategory.Positive);
        var neutralCount = entries.Count(e => e.PrimaryMood.Category == MoodCategory.Neutral);
        var negativeCount = entries.Count(e => e.PrimaryMood.Category == MoodCategory.Negative);

        return new MoodDistribution
        {
            PositiveCount = positiveCount,
            NeutralCount = neutralCount,
            NegativeCount = negativeCount
        };
    }

    public async Task<Mood?> GetMostFrequentMoodAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.JournalEntries
            .Include(e => e.PrimaryMood)
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(e => e.Date >= startDate.Value.Date);
        if (endDate.HasValue)
            query = query.Where(e => e.Date <= endDate.Value.Date);

        var mostFrequent = await query
            .GroupBy(e => e.PrimaryMoodId)
            .Select(g => new { MoodId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .FirstOrDefaultAsync();

        if (mostFrequent == null)
            return null;

        return await _context.Moods.FindAsync(mostFrequent.MoodId);
    }

    public async Task<List<TagUsage>> GetMostUsedTagsAsync(int count = 10, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.JournalEntryTags
            .Include(t => t.Tag)
            .Include(t => t.JournalEntry)
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(t => t.JournalEntry.Date >= startDate.Value.Date);
        if (endDate.HasValue)
            query = query.Where(t => t.JournalEntry.Date <= endDate.Value.Date);

        var totalEntries = await _context.JournalEntries
            .Where(e => (!startDate.HasValue || e.Date >= startDate.Value.Date) &&
                       (!endDate.HasValue || e.Date <= endDate.Value.Date))
            .CountAsync();

        var tagUsage = await query
            .GroupBy(t => new { t.TagId, t.Tag.Name })
            .Select(g => new TagUsage
            {
                TagId = g.Key.TagId,
                TagName = g.Key.Name,
                UsageCount = g.Count(),
                Percentage = totalEntries > 0 ? (g.Count() * 100.0 / totalEntries) : 0
            })
            .OrderByDescending(t => t.UsageCount)
            .Take(count)
            .ToListAsync();

        return tagUsage;
    }

    public async Task<List<TagBreakdown>> GetTagBreakdownAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.JournalEntries.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(e => e.Date >= startDate.Value.Date);
        if (endDate.HasValue)
            query = query.Where(e => e.Date <= endDate.Value.Date);

        var totalEntries = await query.CountAsync();

        var tagBreakdown = await _context.JournalEntryTags
            .Include(t => t.Tag)
            .Include(t => t.JournalEntry)
            .Where(t => (!startDate.HasValue || t.JournalEntry.Date >= startDate.Value.Date) &&
                       (!endDate.HasValue || t.JournalEntry.Date <= endDate.Value.Date))
            .GroupBy(t => new { t.TagId, t.Tag.Name })
            .Select(g => new TagBreakdown
            {
                TagId = g.Key.TagId,
                TagName = g.Key.Name,
                EntryCount = g.Select(t => t.JournalEntryId).Distinct().Count(),
                Percentage = totalEntries > 0 ? (g.Select(t => t.JournalEntryId).Distinct().Count() * 100.0 / totalEntries) : 0
            })
            .OrderByDescending(t => t.EntryCount)
            .ToListAsync();

        return tagBreakdown;
    }

    public async Task<List<WordCountTrend>> GetWordCountTrendsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.JournalEntries.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(e => e.Date >= startDate.Value.Date);
        if (endDate.HasValue)
            query = query.Where(e => e.Date <= endDate.Value.Date);

        var entries = await query
            .OrderBy(e => e.Date)
            .ToListAsync();

        if (!entries.Any())
            return new List<WordCountTrend>();

        // Group by week
        var trends = entries
            .GroupBy(e => GetWeekStart(e.Date))
            .Select(g => new WordCountTrend
            {
                Period = g.Key,
                AverageWordCount = (int)g.Average(e => CountWords(e.Content)),
                EntryCount = g.Count()
            })
            .OrderBy(t => t.Period)
            .ToList();

        return trends;
    }

    private DateTime GetWeekStart(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-1 * diff).Date;
    }

    private int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;
        return text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }
}
