using JournalAppBlazor.Models;
using JournalAppBlazor.Repositories;

namespace JournalAppBlazor.Services;

public interface IMoodService
{
    Task<List<Mood>> GetAllMoodsAsync();
    Task<List<Mood>> GetMoodsByCategoryAsync(MoodCategory category);
    Task<Mood?> GetMoodByIdAsync(int id);
}

public class MoodService : IMoodService
{
    private readonly IMoodRepository _moodRepository;

    public MoodService(IMoodRepository moodRepository)
    {
        _moodRepository = moodRepository;
    }

    public async Task<List<Mood>> GetAllMoodsAsync()
    {
        return await _moodRepository.GetAllOrderedAsync();
    }

    public async Task<List<Mood>> GetMoodsByCategoryAsync(MoodCategory category)
    {
        return await _moodRepository.GetByCategoryAsync(category);
    }

    public async Task<Mood?> GetMoodByIdAsync(int id)
    {
        return await _moodRepository.GetByIdAsync(id);
    }
}
