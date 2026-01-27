using JournalAppBlazor.Models;

namespace JournalAppBlazor.Services;

public interface IAnalyticsService
{
    Task<MoodDistribution> GetMoodDistributionAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<Mood?> GetMostFrequentMoodAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<List<TagUsage>> GetMostUsedTagsAsync(int count = 10, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<TagBreakdown>> GetTagBreakdownAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<List<WordCountTrend>> GetWordCountTrendsAsync(DateTime? startDate = null, DateTime? endDate = null);
}

public class MoodDistribution
{
    public int PositiveCount { get; set; }
    public int NeutralCount { get; set; }
    public int NegativeCount { get; set; }
    public int Total => PositiveCount + NeutralCount + NegativeCount;
    public double PositivePercentage => Total > 0 ? (PositiveCount * 100.0 / Total) : 0;
    public double NeutralPercentage => Total > 0 ? (NeutralCount * 100.0 / Total) : 0;
    public double NegativePercentage => Total > 0 ? (NegativeCount * 100.0 / Total) : 0;
}

public class TagUsage
{
    public int TagId { get; set; }
    public string TagName { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public double Percentage { get; set; }
}

public class TagBreakdown
{
    public int TagId { get; set; }
    public string TagName { get; set; } = string.Empty;
    public int EntryCount { get; set; }
    public double Percentage { get; set; }
}

public class WordCountTrend
{
    public DateTime Period { get; set; }
    public int AverageWordCount { get; set; }
    public int EntryCount { get; set; }
}
