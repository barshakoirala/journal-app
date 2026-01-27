namespace JournalAppBlazor.Models;

public class JournalEntryMood
{
    public int Id { get; set; }
    public int JournalEntryId { get; set; }
    public JournalEntry JournalEntry { get; set; } = null!;
    
    public int MoodId { get; set; }
    public Mood Mood { get; set; } = null!;
}
