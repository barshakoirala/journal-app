using JournalAppBlazor.Models;

namespace JournalAppBlazor.Services;

public interface IStreakService
{
    Task<int> GetCurrentStreakAsync();
    Task<int> GetLongestStreakAsync();
    Task<List<DateTime>> GetMissedDaysAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<StreakInfo> GetStreakInfoAsync();
}

public class StreakInfo
{
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public int TotalEntries { get; set; }
    public List<DateTime> RecentMissedDays { get; set; } = new();
}
