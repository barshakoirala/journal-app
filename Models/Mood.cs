namespace JournalAppBlazor.Models;

public enum MoodCategory
{
    Positive,
    Neutral,
    Negative
}

public class Mood
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public MoodCategory Category { get; set; }
    
    // Navigation properties
    public List<JournalEntry> PrimaryEntries { get; set; } = new();
    public List<JournalEntryMood> SecondaryEntries { get; set; } = new();
}
