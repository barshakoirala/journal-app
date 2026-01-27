using Microsoft.EntityFrameworkCore;
using JournalAppBlazor.Models;

namespace JournalAppBlazor.Data;

public class JournalDbContext : DbContext
{
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

        // Configure JournalEntry
        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Date).IsUnique(); // One entry per day
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            
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
            entity.HasIndex(e => e.Name).IsUnique();
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
