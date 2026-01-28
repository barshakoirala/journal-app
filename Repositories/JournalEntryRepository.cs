using Microsoft.EntityFrameworkCore;
using JournalAppBlazor.Data;
using JournalAppBlazor.Models;

namespace JournalAppBlazor.Repositories;

/// <summary>
/// Repository implementation for JournalEntry.
/// </summary>
public class JournalEntryRepository : Repository<JournalEntry>, IJournalEntryRepository
{
    public JournalEntryRepository(JournalDbContext context) : base(context)
    {
    }

    public async Task<JournalEntry?> GetByIdWithIncludesAsync(int id, int userId)
    {
        return await _dbSet
            .Include(e => e.PrimaryMood)
            .Include(e => e.SecondaryMoods)
                .ThenInclude(sm => sm.Mood)
            .Include(e => e.Tags)
                .ThenInclude(t => t.Tag)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.Id == id);
    }

    public async Task<JournalEntry?> GetByDateWithIncludesAsync(DateTime date, int userId)
    {
        var dateOnly = date.Date;
        return await _dbSet
            .Include(e => e.PrimaryMood)
            .Include(e => e.SecondaryMoods)
                .ThenInclude(sm => sm.Mood)
            .Include(e => e.Tags)
                .ThenInclude(t => t.Tag)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.Date.Date == dateOnly);
    }

    public async Task<List<JournalEntry>> GetAllByUserWithIncludesAsync(int userId)
    {
        return await _dbSet
            .Where(e => e.UserId == userId)
            .Include(e => e.PrimaryMood)
            .Include(e => e.SecondaryMoods)
                .ThenInclude(sm => sm.Mood)
            .Include(e => e.Tags)
                .ThenInclude(t => t.Tag)
            .OrderByDescending(e => e.Date)
            .ToListAsync();
    }

    public async Task<JournalEntry?> GetByIdForUpdateAsync(int id, int userId)
    {
        return await _dbSet
            .Include(e => e.SecondaryMoods)
            .Include(e => e.Tags)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.Id == id);
    }

    public async Task<bool> ExistsForDateAsync(DateTime date, int userId)
    {
        var dateOnly = date.Date;
        return await _dbSet.AnyAsync(e => e.UserId == userId && e.Date.Date == dateOnly);
    }

    public async Task<(List<JournalEntry> Entries, int TotalCount)> GetPaginatedAsync(int userId, int pageNumber, int pageSize)
    {
        var query = _dbSet
            .Where(e => e.UserId == userId)
            .Include(e => e.PrimaryMood)
            .Include(e => e.SecondaryMoods)
                .ThenInclude(sm => sm.Mood)
            .Include(e => e.Tags)
                .ThenInclude(t => t.Tag)
            .OrderByDescending(e => e.Date);

        var totalCount = await query.CountAsync();

        var entries = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (entries, totalCount);
    }

    public async Task<(List<JournalEntry> Entries, int TotalCount)> SearchAsync(
        int userId,
        string? searchTerm = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        List<int>? moodIds = null,
        List<int>? tagIds = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var query = _dbSet
            .Where(e => e.UserId == userId)
            .Include(e => e.PrimaryMood)
            .Include(e => e.SecondaryMoods)
                .ThenInclude(sm => sm.Mood)
            .Include(e => e.Tags)
                .ThenInclude(t => t.Tag)
            .AsQueryable();

        // Search by title or content
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchLower = searchTerm.ToLower();
            query = query.Where(e =>
                e.Title.ToLower().Contains(searchLower) ||
                e.Content.ToLower().Contains(searchLower));
        }

        // Filter by date range
        if (startDate.HasValue)
        {
            query = query.Where(e => e.Date >= startDate.Value.Date);
        }
        if (endDate.HasValue)
        {
            query = query.Where(e => e.Date <= endDate.Value.Date);
        }

        // Filter by moods (primary or secondary)
        if (moodIds != null && moodIds.Any())
        {
            query = query.Where(e =>
                moodIds.Contains(e.PrimaryMoodId) ||
                e.SecondaryMoods.Any(sm => moodIds.Contains(sm.MoodId)));
        }

        // Filter by tags
        if (tagIds != null && tagIds.Any())
        {
            query = query.Where(e =>
                e.Tags.Any(t => tagIds.Contains(t.TagId)));
        }

        var totalCount = await query.CountAsync();

        var entries = await query
            .OrderByDescending(e => e.Date)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (entries, totalCount);
    }

    public async Task<List<DateTime>> GetEntryDatesAsync(int userId)
    {
        return await _dbSet
            .Where(e => e.UserId == userId)
            .OrderBy(e => e.Date)
            .Select(e => e.Date.Date)
            .Distinct()
            .ToListAsync();
    }

    public async Task<List<JournalEntry>> GetByDateRangeAsync(int userId, DateTime? startDate, DateTime? endDate)
    {
        var query = _dbSet.Where(e => e.UserId == userId);

        if (startDate.HasValue)
            query = query.Where(e => e.Date >= startDate.Value.Date);
        if (endDate.HasValue)
            query = query.Where(e => e.Date <= endDate.Value.Date);

        return await query.OrderBy(e => e.Date).ToListAsync();
    }

    public async Task<List<JournalEntry>> GetWithPrimaryMoodAsync(int userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _dbSet
            .Where(e => e.UserId == userId)
            .Include(e => e.PrimaryMood)
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(e => e.Date >= startDate.Value.Date);
        if (endDate.HasValue)
            query = query.Where(e => e.Date <= endDate.Value.Date);

        return await query.ToListAsync();
    }
}
