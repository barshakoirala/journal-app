using JournalAppBlazor.Models;
using JournalAppBlazor.Repositories;

namespace JournalAppBlazor.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IJournalEntryTagRepository _journalEntryTagRepository;
    private readonly IMoodRepository _moodRepository;
    private readonly IAuthService _authService;

    public AnalyticsService(
        IJournalEntryRepository journalEntryRepository,
        IJournalEntryTagRepository journalEntryTagRepository,
        IMoodRepository moodRepository,
        IAuthService authService)
    {
        _journalEntryRepository = journalEntryRepository;
        _journalEntryTagRepository = journalEntryTagRepository;
        _moodRepository = moodRepository;
        _authService = authService;
    }

    private int GetCurrentUserId()
    {
        return _authService.CurrentUserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");
    }

    public async Task<MoodDistribution> GetMoodDistributionAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var userId = GetCurrentUserId();

        var entries = await _journalEntryRepository.GetWithPrimaryMoodAsync(userId, startDate, endDate);

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
        var userId = GetCurrentUserId();

        var entries = await _journalEntryRepository.GetWithPrimaryMoodAsync(userId, startDate, endDate);

        if (!entries.Any())
            return null;

        var mostFrequentMoodId = entries
            .GroupBy(e => e.PrimaryMoodId)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();

        return await _moodRepository.GetByIdAsync(mostFrequentMoodId);
    }

    public async Task<List<TagUsage>> GetMostUsedTagsAsync(int count = 10, DateTime? startDate = null, DateTime? endDate = null)
    {
        var userId = GetCurrentUserId();

        var tagUsageData = await _journalEntryTagRepository.GetTagUsageAsync(userId, startDate, endDate, count);
        
        var totalEntries = await _journalEntryRepository.CountAsync(e => 
            e.UserId == userId &&
            (!startDate.HasValue || e.Date >= startDate.Value.Date) &&
            (!endDate.HasValue || e.Date <= endDate.Value.Date));

        return tagUsageData.Select(t => new TagUsage
        {
            TagId = t.TagId,
            TagName = t.TagName,
            UsageCount = t.UsageCount,
            Percentage = totalEntries > 0 ? (t.UsageCount * 100.0 / totalEntries) : 0
        }).ToList();
    }

    public async Task<List<TagBreakdown>> GetTagBreakdownAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var userId = GetCurrentUserId();

        var tagUsageData = await _journalEntryTagRepository.GetTagUsageAsync(userId, startDate, endDate);
        
        var totalEntries = await _journalEntryRepository.CountAsync(e => 
            e.UserId == userId &&
            (!startDate.HasValue || e.Date >= startDate.Value.Date) &&
            (!endDate.HasValue || e.Date <= endDate.Value.Date));

        return tagUsageData.Select(t => new TagBreakdown
        {
            TagId = t.TagId,
            TagName = t.TagName,
            EntryCount = t.UsageCount,
            Percentage = totalEntries > 0 ? (t.UsageCount * 100.0 / totalEntries) : 0
        }).OrderByDescending(t => t.EntryCount).ToList();
    }

    public async Task<List<WordCountTrend>> GetWordCountTrendsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var userId = GetCurrentUserId();

        var entries = await _journalEntryRepository.GetByDateRangeAsync(userId, startDate, endDate);

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
