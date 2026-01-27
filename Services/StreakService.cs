using Microsoft.EntityFrameworkCore;
using JournalAppBlazor.Data;
using JournalAppBlazor.Models;

namespace JournalAppBlazor.Services;

public class StreakService : IStreakService
{
    private readonly JournalDbContext _context;
    private readonly IAuthService _authService;

    public StreakService(JournalDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    private int GetCurrentUserId()
    {
        return _authService.CurrentUserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");
    }

    public async Task<int> GetCurrentStreakAsync()
    {
        var userId = GetCurrentUserId();
        var today = DateTime.Today;
        var streak = 0;
        var currentDate = today;

        // Check if today has an entry
        var hasEntryToday = await _context.JournalEntries
            .AnyAsync(e => e.UserId == userId && e.Date.Date == currentDate);
        if (!hasEntryToday)
        {
            // If today doesn't have an entry, start from yesterday
            currentDate = today.AddDays(-1);
        }

        // Count consecutive days backwards
        while (currentDate >= DateTime.MinValue.AddDays(1))
        {
            var hasEntry = await _context.JournalEntries
                .AnyAsync(e => e.UserId == userId && e.Date.Date == currentDate);
            if (hasEntry)
            {
                streak++;
                currentDate = currentDate.AddDays(-1);
            }
            else
            {
                break;
            }
        }

        return streak;
    }

    public async Task<int> GetLongestStreakAsync()
    {
        var userId = GetCurrentUserId();

        var entries = await _context.JournalEntries
            .Where(e => e.UserId == userId)
            .OrderBy(e => e.Date)
            .Select(e => e.Date.Date)
            .Distinct()
            .ToListAsync();

        if (!entries.Any())
            return 0;

        var longestStreak = 1;
        var currentStreak = 1;
        var sortedDates = entries.OrderBy(d => d).ToList();

        for (int i = 1; i < sortedDates.Count; i++)
        {
            var daysDiff = (sortedDates[i] - sortedDates[i - 1]).Days;
            if (daysDiff == 1)
            {
                // Consecutive day
                currentStreak++;
                longestStreak = Math.Max(longestStreak, currentStreak);
            }
            else
            {
                // Gap found, reset current streak
                currentStreak = 1;
            }
        }

        return longestStreak;
    }

    public async Task<List<DateTime>> GetMissedDaysAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var userId = GetCurrentUserId();
        var start = startDate?.Date ?? DateTime.Today.AddDays(-30); // Default: last 30 days
        var end = endDate?.Date ?? DateTime.Today;

        // Get all dates with entries in the range
        var entryDates = await _context.JournalEntries
            .Where(e => e.UserId == userId && e.Date.Date >= start && e.Date.Date <= end)
            .Select(e => e.Date.Date)
            .Distinct()
            .ToListAsync();

        // Find all dates in range without entries
        var missedDays = new List<DateTime>();
        var currentDate = start;

        while (currentDate <= end)
        {
            if (!entryDates.Contains(currentDate))
            {
                missedDays.Add(currentDate);
            }
            currentDate = currentDate.AddDays(1);
        }

        return missedDays.OrderByDescending(d => d).ToList();
    }

    public async Task<StreakInfo> GetStreakInfoAsync()
    {
        var userId = GetCurrentUserId();

        var currentStreak = await GetCurrentStreakAsync();
        var longestStreak = await GetLongestStreakAsync();
        var totalEntries = await _context.JournalEntries
            .CountAsync(e => e.UserId == userId);
        var missedDays = await GetMissedDaysAsync(DateTime.Today.AddDays(-7), DateTime.Today); // Last 7 days

        return new StreakInfo
        {
            CurrentStreak = currentStreak,
            LongestStreak = longestStreak,
            TotalEntries = totalEntries,
            RecentMissedDays = missedDays.Take(10).ToList() // Show up to 10 recent missed days
        };
    }
}
