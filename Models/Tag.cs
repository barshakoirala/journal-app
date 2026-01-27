namespace JournalAppBlazor.Models;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsPreBuilt { get; set; }

    // User relationship (null for pre-built tags, set for custom user tags)
    public int? UserId { get; set; }
    public User? User { get; set; }

    // Navigation properties
    public List<JournalEntryTag> JournalEntries { get; set; } = new();
}
