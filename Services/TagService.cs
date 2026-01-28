using JournalAppBlazor.Models;
using JournalAppBlazor.Repositories;

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
    private readonly ITagRepository _tagRepository;
    private readonly IAuthService _authService;

    public TagService(ITagRepository tagRepository, IAuthService authService)
    {
        _tagRepository = tagRepository;
        _authService = authService;
    }

    private int? GetCurrentUserId()
    {
        return _authService.CurrentUserId;
    }

    public async Task<List<Tag>> GetAllTagsAsync()
    {
        var userId = GetCurrentUserId();
        return await _tagRepository.GetAllForUserAsync(userId);
    }

    public async Task<List<Tag>> GetPreBuiltTagsAsync()
    {
        return await _tagRepository.GetPreBuiltAsync();
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

        await _tagRepository.AddAsync(tag);
        await _tagRepository.SaveChangesAsync();
        return tag;
    }

    public async Task<Tag?> GetTagByIdAsync(int id)
    {
        var userId = GetCurrentUserId();
        return await _tagRepository.GetByIdForUserAsync(id, userId);
    }

    public async Task<Tag?> GetTagByNameAsync(string name)
    {
        var userId = GetCurrentUserId();
        return await _tagRepository.GetByNameForUserAsync(name, userId);
    }
}
