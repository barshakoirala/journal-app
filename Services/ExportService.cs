using JournalAppBlazor.Models;
using JournalAppBlazor.Repositories;
using System.Text;

namespace JournalAppBlazor.Services;

public class ExportService : IExportService
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IAuthService _authService;

    public ExportService(IJournalEntryRepository journalEntryRepository, IAuthService authService)
    {
        _journalEntryRepository = journalEntryRepository;
        _authService = authService;
    }

    private int GetCurrentUserId()
    {
        return _authService.CurrentUserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");
    }

    public async Task<string> GenerateHtmlAsync(DateTime startDate, DateTime endDate)
    {
        var userId = GetCurrentUserId();
        var username = _authService.CurrentUsername ?? "User";

        // Use repository's search method to get entries with all includes
        var (entries, _) = await _journalEntryRepository.SearchAsync(
            userId,
            searchTerm: null,
            startDate: startDate,
            endDate: endDate,
            moodIds: null,
            tagIds: null,
            pageNumber: 1,
            pageSize: int.MaxValue);

        // Sort by date descending
        entries = entries.OrderByDescending(e => e.Date).ToList();

        var html = new StringBuilder();
        html.AppendLine($@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <title>{Esc(username)}'s Journal</title>
    <style>
        @page {{ margin: 0.75in; }}
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Helvetica Neue', sans-serif; max-width: 100%; padding: 20px; color: #333; font-size: 14px; line-height: 1.5; }}
        .header {{ text-align: center; margin-bottom: 30px; padding-bottom: 15px; border-bottom: 2px solid #6366f1; }}
        .header h1 {{ font-size: 24px; color: #4f46e5; margin-bottom: 8px; }}
        .header p {{ color: #6b7280; font-size: 13px; }}
        .entry {{ border: 1px solid #e5e7eb; border-radius: 10px; margin-bottom: 20px; overflow: hidden; page-break-inside: avoid; }}
        .entry-header {{ background: #f9fafb; padding: 14px 18px; border-bottom: 1px solid #e5e7eb; }}
        .entry-title {{ font-size: 16px; font-weight: 600; color: #1e40af; margin-bottom: 4px; }}
        .entry-date {{ font-size: 12px; color: #6b7280; }}
        .entry-body {{ padding: 16px 18px; }}
        .moods {{ margin-bottom: 10px; }}
        .mood {{ display: inline-block; padding: 4px 12px; border-radius: 14px; font-size: 12px; font-weight: 500; color: white; margin-right: 6px; }}
        .mood-positive {{ background: #22c55e; }}
        .mood-neutral {{ background: #3b82f6; }}
        .mood-negative {{ background: #ef4444; }}
        .tags {{ margin-bottom: 12px; }}
        .tag {{ display: inline-block; padding: 3px 10px; border: 1px solid #d1d5db; border-radius: 5px; font-size: 11px; color: #4b5563; margin-right: 5px; }}
        .content {{ white-space: pre-wrap; line-height: 1.7; font-size: 14px; }}
        .no-entries {{ text-align: center; padding: 50px; color: #6b7280; }}
        .print-tip {{ background: #fef3c7; border: 1px solid #fbbf24; padding: 12px 16px; border-radius: 8px; margin-bottom: 20px; font-size: 13px; }}
        @media print {{ .print-tip {{ display: none; }} }}
    </style>
</head>
<body>
    <div class=""print-tip"">
        <strong>To save as PDF:</strong> Press Cmd+P → Select ""Save as PDF"" from the dropdown
    </div>
    <div class=""header"">
        <h1>{Esc(username)}'s Journal</h1>
        <p>{startDate:MMMM dd, yyyy} — {endDate:MMMM dd, yyyy} &nbsp;•&nbsp; {entries.Count} {(entries.Count == 1 ? "entry" : "entries")}</p>
    </div>");

        if (!entries.Any())
        {
            html.AppendLine(@"    <div class=""no-entries"">No journal entries found for this date range.</div>");
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
            <div class=""entry-title"">{Esc(entry.Title)}</div>
            <div class=""entry-date"">{entry.Date:dddd, MMMM dd, yyyy}</div>
        </div>
        <div class=""entry-body"">
            <div class=""moods"">
                <span class=""mood {moodClass}"">{Esc(entry.PrimaryMood.Name)}</span>");

                foreach (var sm in entry.SecondaryMoods.Take(2))
                {
                    var smClass = sm.Mood.Category switch
                    {
                        MoodCategory.Positive => "mood-positive",
                        MoodCategory.Neutral => "mood-neutral",
                        MoodCategory.Negative => "mood-negative",
                        _ => "mood-neutral"
                    };
                    html.AppendLine($@"                <span class=""mood {smClass}"">{Esc(sm.Mood.Name)}</span>");
                }

                html.AppendLine(@"            </div>");

                if (entry.Tags.Any())
                {
                    html.AppendLine(@"            <div class=""tags"">");
                    foreach (var t in entry.Tags)
                    {
                        html.AppendLine($@"                <span class=""tag"">#{Esc(t.Tag.Name)}</span>");
                    }
                    html.AppendLine(@"            </div>");
                }

                html.AppendLine($@"            <div class=""content"">{Esc(entry.Content)}</div>
        </div>
    </div>");
            }
        }

        html.AppendLine(@"</body></html>");
        return html.ToString();
    }

    private string Esc(string text) => string.IsNullOrEmpty(text) ? "" : 
        text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
}
