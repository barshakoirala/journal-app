using Microsoft.EntityFrameworkCore;
using JournalAppBlazor.Data;
using JournalAppBlazor.Models;

namespace JournalAppBlazor.Services;

public class JournalService : IJournalService
{
    private readonly JournalDbContext _context;

    public JournalService(JournalDbContext context)
    {
        _context = context;
    }

    public async Task<JournalEntry?> GetEntryByDateAsync(DateTime date)
    {
        var dateOnly = date.Date;
        return await _context.JournalEntries
            .Include(e => e.PrimaryMood)
            .Include(e => e.SecondaryMoods)
                .ThenInclude(sm => sm.Mood)
            .Include(e => e.Tags)
                .ThenInclude(t => t.Tag)
            .FirstOrDefaultAsync(e => e.Date.Date == dateOnly);
    }

    public async Task<JournalEntry?> GetEntryByIdAsync(int id)
    {
        return await _context.JournalEntries
            .Include(e => e.PrimaryMood)
            .Include(e => e.SecondaryMoods)
                .ThenInclude(sm => sm.Mood)
            .Include(e => e.Tags)
                .ThenInclude(t => t.Tag)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<List<JournalEntry>> GetAllEntriesAsync()
    {
        return await _context.JournalEntries
            .Include(e => e.PrimaryMood)
            .Include(e => e.SecondaryMoods)
                .ThenInclude(sm => sm.Mood)
            .Include(e => e.Tags)
                .ThenInclude(t => t.Tag)
            .OrderByDescending(e => e.Date)
            .ToListAsync();
    }

    public async Task<JournalEntry> CreateEntryAsync(JournalEntry entry, List<int>? secondaryMoodIds = null, List<int>? tagIds = null)
    {
        // Ensure date is set to today if not provided
        if (entry.Date == default)
        {
            entry.Date = DateTime.Today;
        }
        else
        {
            entry.Date = entry.Date.Date; // Normalize to date only
        }

        // Validate PrimaryMoodId is set
        if (entry.PrimaryMoodId <= 0)
        {
            throw new InvalidOperationException("Primary mood is required.");
        }

        // Check if entry already exists for this date
        if (await EntryExistsForDateAsync(entry.Date))
        {
            throw new InvalidOperationException($"An entry already exists for {entry.Date:yyyy-MM-dd}. Only one entry per day is allowed.");
        }

        entry.CreatedAt = DateTime.Now;
        entry.UpdatedAt = DateTime.Now;

        // Clear navigation properties to avoid tracking issues - EF will set them based on foreign keys
        entry.PrimaryMood = null!;
        entry.SecondaryMoods.Clear();
        entry.Tags.Clear();

        try
        {
            _context.JournalEntries.Add(entry);
            await _context.SaveChangesAsync();

            // Add secondary moods (up to 2)
            if (secondaryMoodIds != null && secondaryMoodIds.Any())
            {
                var moodsToAdd = secondaryMoodIds.Take(2).ToList();
                foreach (var moodId in moodsToAdd)
                {
                    _context.JournalEntryMoods.Add(new JournalEntryMood
                    {
                        JournalEntryId = entry.Id,
                        MoodId = moodId
                    });
                }
            }

            // Add tags
            if (tagIds != null && tagIds.Any())
            {
                foreach (var tagId in tagIds)
                {
                    _context.JournalEntryTags.Add(new JournalEntryTag
                    {
                        JournalEntryId = entry.Id,
                        TagId = tagId
                    });
                }
            }

            await _context.SaveChangesAsync();
            return await GetEntryByIdAsync(entry.Id) ?? entry;
        }
        catch (DbUpdateException ex)
        {
            // Preserve the full exception chain for better error reporting
            var errorMessage = $"Database error: {ex.Message}";
            if (ex.InnerException != null)
            {
                errorMessage += $" | Inner: {ex.InnerException.Message}";
            }
            throw new InvalidOperationException(errorMessage, ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Unexpected error saving entry: {ex.Message}", ex);
        }
    }

    public async Task<JournalEntry> UpdateEntryAsync(JournalEntry entry, List<int>? secondaryMoodIds = null, List<int>? tagIds = null)
    {
        var existingEntry = await _context.JournalEntries
            .Include(e => e.SecondaryMoods)
            .Include(e => e.Tags)
            .FirstOrDefaultAsync(e => e.Id == entry.Id);

        if (existingEntry == null)
        {
            throw new InvalidOperationException($"Entry with ID {entry.Id} not found.");
        }

        // Update properties
        existingEntry.Title = entry.Title;
        existingEntry.Content = entry.Content;
        existingEntry.PrimaryMoodId = entry.PrimaryMoodId;
        existingEntry.UpdatedAt = DateTime.Now;

        // Update secondary moods
        _context.JournalEntryMoods.RemoveRange(existingEntry.SecondaryMoods);
        if (secondaryMoodIds != null && secondaryMoodIds.Any())
        {
            var moodsToAdd = secondaryMoodIds.Take(2).ToList();
            foreach (var moodId in moodsToAdd)
            {
                _context.JournalEntryMoods.Add(new JournalEntryMood
                {
                    JournalEntryId = existingEntry.Id,
                    MoodId = moodId
                });
            }
        }

        // Update tags
        _context.JournalEntryTags.RemoveRange(existingEntry.Tags);
        if (tagIds != null && tagIds.Any())
        {
            foreach (var tagId in tagIds)
            {
                _context.JournalEntryTags.Add(new JournalEntryTag
                {
                    JournalEntryId = existingEntry.Id,
                    TagId = tagId
                });
            }
        }

        await _context.SaveChangesAsync();
        return await GetEntryByIdAsync(existingEntry.Id) ?? existingEntry;
    }

    public async Task DeleteEntryAsync(int id)
    {
        var entry = await _context.JournalEntries.FindAsync(id);
        if (entry != null)
        {
            _context.JournalEntries.Remove(entry);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> EntryExistsForDateAsync(DateTime date)
    {
        var dateOnly = date.Date;
        return await _context.JournalEntries.AnyAsync(e => e.Date.Date == dateOnly);
    }

    public async Task<(List<JournalEntry> Entries, int TotalCount)> GetEntriesPaginatedAsync(int pageNumber, int pageSize)
    {
        var query = _context.JournalEntries
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

    public async Task<(List<JournalEntry> Entries, int TotalCount)> SearchEntriesAsync(
        string? searchTerm = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        List<int>? moodIds = null,
        List<int>? tagIds = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var query = _context.JournalEntries
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
}
