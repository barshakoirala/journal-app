namespace JournalAppBlazor.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public List<JournalEntry> JournalEntries { get; set; } = new();
    public List<Tag> Tags { get; set; } = new();
}
