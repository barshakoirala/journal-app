using JournalAppBlazor.Models;
using JournalAppBlazor.Repositories;

namespace JournalAppBlazor.Services;

public class JournalService : IJournalService
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IJournalEntryMoodRepository _journalEntryMoodRepository;
    private readonly IJournalEntryTagRepository _journalEntryTagRepository;
    private readonly IAuthService _authService;

    public JournalService(
        IJournalEntryRepository journalEntryRepository,
        IJournalEntryMoodRepository journalEntryMoodRepository,
        IJournalEntryTagRepository journalEntryTagRepository,
        IAuthService authService)
    {
        _journalEntryRepository = journalEntryRepository;
        _journalEntryMoodRepository = journalEntryMoodRepository;
        _journalEntryTagRepository = journalEntryTagRepository;
        _authService = authService;
    }

    private int GetCurrentUserId()
    {
        return _authService.CurrentUserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");
    }

    public async Task<JournalEntry?> GetEntryByDateAsync(DateTime date)
    {
        var userId = GetCurrentUserId();
        return await _journalEntryRepository.GetByDateWithIncludesAsync(date, userId);
    }

    public async Task<JournalEntry?> GetEntryByIdAsync(int id)
    {
        var userId = GetCurrentUserId();
        return await _journalEntryRepository.GetByIdWithIncludesAsync(id, userId);
    }

    public async Task<List<JournalEntry>> GetAllEntriesAsync()
    {
        var userId = GetCurrentUserId();
        return await _journalEntryRepository.GetAllByUserWithIncludesAsync(userId);
    }

    public async Task<JournalEntry> CreateEntryAsync(JournalEntry entry, List<int>? secondaryMoodIds = null, List<int>? tagIds = null)
    {
        var userId = GetCurrentUserId();

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

        // Check if entry already exists for this date FOR THIS USER
        if (await EntryExistsForDateAsync(entry.Date))
        {
            throw new InvalidOperationException($"An entry already exists for {entry.Date:yyyy-MM-dd}. Only one entry per day is allowed.");
        }

        // Set user ID
        entry.UserId = userId;
        entry.CreatedAt = DateTime.Now;
        entry.UpdatedAt = DateTime.Now;

        // Clear navigation properties to avoid tracking issues - EF will set them based on foreign keys
        entry.PrimaryMood = null!;
        entry.User = null!;
        entry.SecondaryMoods.Clear();
        entry.Tags.Clear();

        try
        {
            await _journalEntryRepository.AddAsync(entry);
            await _journalEntryRepository.SaveChangesAsync();

            // Add secondary moods (up to 2)
            if (secondaryMoodIds != null && secondaryMoodIds.Any())
            {
                await _journalEntryMoodRepository.AddSecondaryMoodsAsync(entry.Id, secondaryMoodIds);
            }

            // Add tags
            if (tagIds != null && tagIds.Any())
            {
                await _journalEntryTagRepository.AddTagsAsync(entry.Id, tagIds);
            }

            await _journalEntryRepository.SaveChangesAsync();
            return await GetEntryByIdAsync(entry.Id) ?? entry;
        }
        catch (Exception ex) when (ex.GetType().Name == "DbUpdateException")
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
        var userId = GetCurrentUserId();

        var existingEntry = await _journalEntryRepository.GetByIdForUpdateAsync(entry.Id, userId);

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
        await _journalEntryMoodRepository.RemoveByJournalEntryIdAsync(existingEntry.Id);
        if (secondaryMoodIds != null && secondaryMoodIds.Any())
        {
            await _journalEntryMoodRepository.AddSecondaryMoodsAsync(existingEntry.Id, secondaryMoodIds);
        }

        // Update tags
        await _journalEntryTagRepository.RemoveByJournalEntryIdAsync(existingEntry.Id);
        if (tagIds != null && tagIds.Any())
        {
            await _journalEntryTagRepository.AddTagsAsync(existingEntry.Id, tagIds);
        }

        await _journalEntryRepository.SaveChangesAsync();
        return await GetEntryByIdAsync(existingEntry.Id) ?? existingEntry;
    }

    public async Task DeleteEntryAsync(int id)
    {
        var userId = GetCurrentUserId();
        var entry = await _journalEntryRepository.FirstOrDefaultAsync(e => e.UserId == userId && e.Id == id);

        if (entry != null)
        {
            _journalEntryRepository.Remove(entry);
            await _journalEntryRepository.SaveChangesAsync();
        }
    }

    public async Task<bool> EntryExistsForDateAsync(DateTime date)
    {
        var userId = GetCurrentUserId();
        return await _journalEntryRepository.ExistsForDateAsync(date, userId);
    }

    public async Task<(List<JournalEntry> Entries, int TotalCount)> GetEntriesPaginatedAsync(int pageNumber, int pageSize)
    {
        var userId = GetCurrentUserId();
        return await _journalEntryRepository.GetPaginatedAsync(userId, pageNumber, pageSize);
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
        var userId = GetCurrentUserId();
        return await _journalEntryRepository.SearchAsync(userId, searchTerm, startDate, endDate, moodIds, tagIds, pageNumber, pageSize);
    }
}
