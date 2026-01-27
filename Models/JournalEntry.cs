namespace JournalAppBlazor.Models;

public class JournalEntry
{
    public int Id { get; set; }
    public DateTime Date { get; set; } // Date only (one entry per day)
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public int PrimaryMoodId { get; set; }
    public Mood PrimaryMood { get; set; } = null!;
    
    public List<JournalEntryMood> SecondaryMoods { get; set; } = new();
    public List<JournalEntryTag> Tags { get; set; } = new();
}
