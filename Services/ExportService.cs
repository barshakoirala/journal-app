using JournalAppBlazor.Models;
using Microsoft.EntityFrameworkCore;
using JournalAppBlazor.Data;
using System.Text;

namespace JournalAppBlazor.Services;

public class ExportService : IExportService
{
    private readonly IDbContextFactory<JournalDbContext> _contextFactory;

    public ExportService(IDbContextFactory<JournalDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<string> ExportToHtmlAsync(DateTime startDate, DateTime endDate)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var entries = await context.JournalEntries
            .Include(e => e.PrimaryMood)
            .Include(e => e.SecondaryMoods)
                .ThenInclude(sm => sm.Mood)
            .Include(e => e.Tags)
                .ThenInclude(t => t.Tag)
            .Where(e => e.Date >= startDate.Date && e.Date <= endDate.Date)
            .OrderByDescending(e => e.Date)
            .ToListAsync();

        var html = new StringBuilder();
        
        // HTML header with print-friendly CSS
        html.AppendLine(@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Journal Export</title>
    <style>
        * { box-sizing: border-box; }
        body { 
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 800px;
            margin: 0 auto;
            padding: 20px;
            background: #fff;
        }
        .header {
            text-align: center;
            border-bottom: 2px solid #3b82f6;
            padding-bottom: 20px;
            margin-bottom: 30px;
        }
        .header h1 { color: #1e40af; margin: 0; }
        .header p { color: #6b7280; margin: 5px 0 0 0; }
        .summary {
            display: flex;
            justify-content: space-between;
            background: #f3f4f6;
            padding: 10px 15px;
            border-radius: 8px;
            margin-bottom: 20px;
            font-size: 14px;
            color: #4b5563;
        }
        .entry {
            border: 1px solid #e5e7eb;
            border-radius: 12px;
            margin-bottom: 20px;
            overflow: hidden;
            page-break-inside: avoid;
        }
        .entry-header {
            background: #f9fafb;
            padding: 15px;
            border-bottom: 1px solid #e5e7eb;
        }
        .entry-title {
            font-size: 18px;
            font-weight: 600;
            color: #1e40af;
            margin: 0;
        }
        .entry-date {
            font-size: 13px;
            color: #6b7280;
            margin-top: 5px;
        }
        .entry-body { padding: 15px; }
        .moods {
            display: flex;
            gap: 8px;
            flex-wrap: wrap;
            margin-bottom: 10px;
        }
        .mood {
            display: inline-block;
            padding: 4px 10px;
            border-radius: 20px;
            font-size: 12px;
            font-weight: 500;
            color: white;
        }
        .mood-positive { background: #22c55e; }
        .mood-neutral { background: #3b82f6; }
        .mood-negative { background: #ef4444; }
        .tags {
            display: flex;
            gap: 6px;
            flex-wrap: wrap;
            margin-bottom: 15px;
        }
        .tag {
            display: inline-block;
            padding: 3px 8px;
            border: 1px solid #d1d5db;
            border-radius: 4px;
            font-size: 11px;
            color: #4b5563;
        }
        .content {
            white-space: pre-wrap;
            line-height: 1.8;
        }
        .entry-footer {
            font-size: 11px;
            color: #9ca3af;
            margin-top: 15px;
            padding-top: 10px;
            border-top: 1px solid #f3f4f6;
        }
        .no-entries {
            text-align: center;
            padding: 50px;
            color: #6b7280;
        }
        .print-note {
            background: #fef3c7;
            border: 1px solid #f59e0b;
            border-radius: 8px;
            padding: 15px;
            margin-bottom: 20px;
            font-size: 14px;
        }
        @media print {
            .print-note { display: none; }
            body { padding: 0; }
            .entry { break-inside: avoid; }
        }
    </style>
</head>
<body>");

        // Header
        html.AppendLine(@"
    <div class=""header"">
        <h1>My Journal</h1>
        <p>Personal Journal Export</p>
    </div>");

        // Print instructions
        html.AppendLine(@"
    <div class=""print-note"">
        <strong>To save as PDF:</strong> Press <kbd>Cmd+P</kbd> (Mac) or <kbd>Ctrl+P</kbd> (Windows), then select ""Save as PDF"" as the destination.
    </div>");

        // Summary
        html.AppendLine($@"
    <div class=""summary"">
        <span>Date Range: {startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}</span>
        <span>Total Entries: {entries.Count}</span>
    </div>");

        if (entries.Count == 0)
        {
            html.AppendLine(@"
    <div class=""no-entries"">
        <p>No journal entries found for this date range.</p>
    </div>");
        }
        else
        {
            foreach (var entry in entries)
            {
                var moodClass = entry.PrimaryMood.Category switch
                {
                    MoodCategory.Positive => "mood-positive",
                    MoodCategory.Neutral => "mood-neutral",
                    MoodCategory.Negative => "mood-negative",
                    _ => "mood-neutral"
                };

                html.AppendLine($@"
    <div class=""entry"">
        <div class=""entry-header"">
            <h2 class=""entry-title"">{EscapeHtml(entry.Title)}</h2>
            <div class=""entry-date"">{entry.Date:dddd, MMMM dd, yyyy}</div>
        </div>
        <div class=""entry-body"">
            <div class=""moods"">
                <span class=""mood {moodClass}"">{EscapeHtml(entry.PrimaryMood.Name)}</span>");

                foreach (var secondaryMood in entry.SecondaryMoods.Take(2))
                {
                    var secMoodClass = secondaryMood.Mood.Category switch
                    {
                        MoodCategory.Positive => "mood-positive",
                        MoodCategory.Neutral => "mood-neutral",
                        MoodCategory.Negative => "mood-negative",
                        _ => "mood-neutral"
                    };
                    html.AppendLine($@"                <span class=""mood {secMoodClass}"">{EscapeHtml(secondaryMood.Mood.Name)}</span>");
                }

                html.AppendLine(@"            </div>");

                if (entry.Tags.Any())
                {
                    html.AppendLine(@"            <div class=""tags"">");
                    foreach (var tag in entry.Tags)
                    {
                        html.AppendLine($@"                <span class=""tag"">#{EscapeHtml(tag.Tag.Name)}</span>");
                    }
                    html.AppendLine(@"            </div>");
                }

                html.AppendLine($@"            <div class=""content"">{EscapeHtml(entry.Content)}</div>
            <div class=""entry-footer"">
                Created: {entry.CreatedAt:MMM dd, yyyy h:mm tt}");

                if (entry.UpdatedAt != entry.CreatedAt)
                {
                    html.AppendLine($@"                | Updated: {entry.UpdatedAt:MMM dd, yyyy h:mm tt}");
                }

                html.AppendLine(@"            </div>
        </div>
    </div>");
            }
        }

        // Close HTML
        html.AppendLine(@"
</body>
</html>");

        return html.ToString();
    }

    private string EscapeHtml(string text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }

    public async Task<string> SaveExportToFileAsync(string content, string fileName)
    {
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var filePath = Path.Combine(documentsPath, fileName);
        
        await File.WriteAllTextAsync(filePath, content);
        
        return filePath;
    }
}
