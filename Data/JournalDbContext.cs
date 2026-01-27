using Microsoft.EntityFrameworkCore;
using JournalAppBlazor.Models;

namespace JournalAppBlazor.Data;

public class JournalDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<JournalEntry> JournalEntries { get; set; }
    public DbSet<Mood> Moods { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<JournalEntryMood> JournalEntryMoods { get; set; }
    public DbSet<JournalEntryTag> JournalEntryTags { get; set; }

    public JournalDbContext(DbContextOptions<JournalDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        // Configure JournalEntry
        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.Date }).IsUnique(); // One entry per day PER USER
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            // User relationship
            entity.HasOne(e => e.User)
                  .WithMany(u => u.JournalEntries)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // One-to-many: Primary Mood
            entity.HasOne(e => e.PrimaryMood)
                  .WithMany(m => m.PrimaryEntries)
                  .HasForeignKey(e => e.PrimaryMoodId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Mood
        modelBuilder.Entity<Mood>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configure Tag
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            // Unique tag names per user (UserId can be null for pre-built tags)
            entity.HasIndex(e => new { e.UserId, e.Name }).IsUnique();

            // User relationship (optional - null for pre-built tags)
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Tags)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .IsRequired(false);
        });

        // Configure JournalEntryMood (junction table)
        modelBuilder.Entity<JournalEntryMood>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.JournalEntry)
                  .WithMany(j => j.SecondaryMoods)
                  .HasForeignKey(e => e.JournalEntryId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Mood)
                  .WithMany(m => m.SecondaryEntries)
                  .HasForeignKey(e => e.MoodId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure JournalEntryTag (junction table)
        modelBuilder.Entity<JournalEntryTag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.JournalEntry)
                  .WithMany(j => j.Tags)
                  .HasForeignKey(e => e.JournalEntryId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Tag)
                  .WithMany(t => t.JournalEntries)
                  .HasForeignKey(e => e.TagId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
