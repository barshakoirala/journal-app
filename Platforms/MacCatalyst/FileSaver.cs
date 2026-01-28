using Foundation;
using UIKit;

namespace JournalAppBlazor.Services;

public class FileSaver : IFileSaver
{
    public async Task<string?> SaveFileWithDialogAsync(byte[] content, string suggestedFileName)
    {
        var tcs = new TaskCompletionSource<string?>();

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            try
            {
                // Save to temp location first
                var tempDir = Path.Combine(Path.GetTempPath(), "JournalExport");
                Directory.CreateDirectory(tempDir);
                var tempPath = Path.Combine(tempDir, suggestedFileName);
                await File.WriteAllBytesAsync(tempPath, content);
                
                var tempUrl = NSUrl.FromFilename(tempPath);

                // Use UIDocumentPickerViewController in MoveToService mode for "Save As" dialog
                var picker = new UIDocumentPickerViewController(new[] { tempUrl }, false);
                picker.DirectoryUrl = NSUrl.FromFilename(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                
                picker.DidPickDocumentAtUrls += (s, e) =>
                {
                    try
                    {
                        if (e.Urls?.Length > 0)
                        {
                            tcs.TrySetResult(e.Urls[0].Path);
                        }
                        else
                        {
                            tcs.TrySetResult(tempPath); // Fall back to temp path
                        }
                    }
                    catch
                    {
                        tcs.TrySetResult(tempPath);
                    }
                };

                picker.WasCancelled += (s, e) =>
                {
                    tcs.TrySetResult(null);
                };

                var vc = Platform.GetCurrentUIViewController();
                if (vc != null)
                {
                    await vc.PresentViewControllerAsync(picker, true);
                }
                else
                {
                    // Fallback: just save to Documents
                    var docPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), suggestedFileName);
                    await File.WriteAllBytesAsync(docPath, content);
                    tcs.TrySetResult(docPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FileSaver error: {ex}");
                tcs.TrySetException(ex);
            }
        });

        return await tcs.Task;
    }
}
