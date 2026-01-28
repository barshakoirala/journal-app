namespace JournalAppBlazor.Services;

public interface IFileSaver
{
    Task<string?> SaveFileWithDialogAsync(byte[] content, string suggestedFileName);
}
