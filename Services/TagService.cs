using Microsoft.EntityFrameworkCore;
using JournalAppBlazor.Data;
using JournalAppBlazor.Models;

namespace JournalAppBlazor.Services;

public interface ITagService
{
    Task<List<Tag>> GetAllTagsAsync();
    Task<List<Tag>> GetPreBuiltTagsAsync();
    Task<Tag> CreateTagAsync(string name);
    Task<Tag?> GetTagByIdAsync(int id);
    Task<Tag?> GetTagByNameAsync(string name);
}

public class TagService : ITagService
{
    private readonly JournalDbContext _context;
    private readonly IAuthService _authService;

    public TagService(JournalDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    private int? GetCurrentUserId()
    {
        return _authService.CurrentUserId;
    }

    public async Task<List<Tag>> GetAllTagsAsync()
    {
        var userId = GetCurrentUserId();

        // Return pre-built tags (UserId is null) + user's custom tags
        return await _context.Tags
            .Where(t => t.IsPreBuilt || t.UserId == userId)
            .OrderBy(t => t.IsPreBuilt ? 0 : 1)
            .ThenBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<List<Tag>> GetPreBuiltTagsAsync()
    {
        return await _context.Tags
            .Where(t => t.IsPreBuilt)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<Tag> CreateTagAsync(string name)
    {
        var userId = GetCurrentUserId();

        // Check if tag already exists (either pre-built or user's own)
        var existingTag = await GetTagByNameAsync(name);
        if (existingTag != null)
        {
            return existingTag;
        }

        var tag = new Tag
        {
            Name = name,
            IsPreBuilt = false,
            UserId = userId
        };

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();
        return tag;
    }

    public async Task<Tag?> GetTagByIdAsync(int id)
    {
        var userId = GetCurrentUserId();

        // Allow access to pre-built tags or user's own tags
        return await _context.Tags
            .FirstOrDefaultAsync(t => t.Id == id && (t.IsPreBuilt || t.UserId == userId));
    }

    public async Task<Tag?> GetTagByNameAsync(string name)
    {
        var userId = GetCurrentUserId();

        // Check pre-built tags first, then user's custom tags
        return await _context.Tags
            .FirstOrDefaultAsync(t =>
                t.Name.ToLower() == name.ToLower() &&
                (t.IsPreBuilt || t.UserId == userId));
    }
}
