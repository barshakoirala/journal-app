using Microsoft.EntityFrameworkCore;
using JournalAppBlazor.Models;

namespace JournalAppBlazor.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(JournalDbContext context)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed moods if they don't exist
        if (!await context.Moods.AnyAsync())
        {
            var moods = new List<Mood>
            {
                // Positive moods
                new Mood { Name = "Happy", Category = MoodCategory.Positive },
                new Mood { Name = "Excited", Category = MoodCategory.Positive },
                new Mood { Name = "Relaxed", Category = MoodCategory.Positive },
                new Mood { Name = "Grateful", Category = MoodCategory.Positive },
                new Mood { Name = "Confident", Category = MoodCategory.Positive },
                
                // Neutral moods
                new Mood { Name = "Calm", Category = MoodCategory.Neutral },
                new Mood { Name = "Thoughtful", Category = MoodCategory.Neutral },
                new Mood { Name = "Curious", Category = MoodCategory.Neutral },
                new Mood { Name = "Nostalgic", Category = MoodCategory.Neutral },
                new Mood { Name = "Bored", Category = MoodCategory.Neutral },
                
                // Negative moods
                new Mood { Name = "Sad", Category = MoodCategory.Negative },
                new Mood { Name = "Angry", Category = MoodCategory.Negative },
                new Mood { Name = "Stressed", Category = MoodCategory.Negative },
                new Mood { Name = "Lonely", Category = MoodCategory.Negative },
                new Mood { Name = "Anxious", Category = MoodCategory.Negative }
            };

            await context.Moods.AddRangeAsync(moods);
        }

        // Seed tags if they don't exist
        if (!await context.Tags.AnyAsync())
        {
            var preBuiltTags = new[]
            {
                "Work", "Career", "Studies", "Family", "Friends", "Relationships",
                "Health", "Fitness", "Personal Growth", "Self-care", "Hobbies",
                "Travel", "Nature", "Finance", "Spirituality", "Birthday", "Holiday",
                "Vacation", "Celebration", "Exercise", "Reading", "Writing", "Cooking",
                "Meditation", "Yoga", "Music", "Shopping", "Parenting", "Projects",
                "Planning", "Reflection"
            };

            var tags = preBuiltTags.Select(name => new Tag
            {
                Name = name,
                IsPreBuilt = true
            }).ToList();

            await context.Tags.AddRangeAsync(tags);
        }

        await context.SaveChangesAsync();
    }
}
