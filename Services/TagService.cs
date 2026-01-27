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

    public TagService(JournalDbContext context)
    {
        _context = context;
    }

    public async Task<List<Tag>> GetAllTagsAsync()
    {
        return await _context.Tags
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
        // Check if tag already exists
        var existingTag = await GetTagByNameAsync(name);
        if (existingTag != null)
        {
            return existingTag;
        }

        var tag = new Tag
        {
            Name = name,
            IsPreBuilt = false
        };

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();
        return tag;
    }

    public async Task<Tag?> GetTagByIdAsync(int id)
    {
        return await _context.Tags.FindAsync(id);
    }

    public async Task<Tag?> GetTagByNameAsync(string name)
    {
        return await _context.Tags
            .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());
    }
}
