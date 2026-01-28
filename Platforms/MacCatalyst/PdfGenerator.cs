using Foundation;
using UIKit;
using WebKit;

namespace JournalAppBlazor.Services;

public class PdfGenerator : IPdfGenerator
{
    public async Task<byte[]> GeneratePdfFromHtmlAsync(string html)
    {
        Console.WriteLine("PdfGenerator: Starting...");
        var tcs = new TaskCompletionSource<byte[]>();

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                Console.WriteLine("PdfGenerator: Creating WKWebView...");
                var config = new WKWebViewConfiguration();
                var webView = new WKWebView(new CoreGraphics.CGRect(0, 0, 612, 792), config);
                
                var loadTcs = new TaskCompletionSource<bool>();
                
                var navigationDelegate = new PdfNavigationDelegate(
                    () => { Console.WriteLine("PdfGenerator: Page loaded"); loadTcs.TrySetResult(true); },
                    (err) => { Console.WriteLine($"PdfGenerator: Load failed - {err}"); loadTcs.TrySetResult(true); }
                );
                webView.NavigationDelegate = navigationDelegate;
                
                Console.WriteLine("PdfGenerator: Loading HTML...");
                webView.LoadHtmlString(html, baseUrl: null!);
                
                // Wait for page to load (with timeout)
                var loadTask = loadTcs.Task;
                var timeoutTask = Task.Delay(15000);
                
                if (await Task.WhenAny(loadTask, timeoutTask) == timeoutTask)
                {
                    Console.WriteLine("PdfGenerator: Timeout!");
                    tcs.TrySetException(new TimeoutException("HTML loading timed out"));
                    webView.Dispose();
                    return;
                }

                // Delay to ensure rendering is complete
                await Task.Delay(1000);

                Console.WriteLine("PdfGenerator: Creating PDF...");
                var pdfConfig = new WKPdfConfiguration();
                pdfConfig.Rect = new CoreGraphics.CGRect(0, 0, 612, 792);
                
                webView.CreatePdf(pdfConfig, (data, error) =>
                {
                    Console.WriteLine($"PdfGenerator: CreatePdf callback - data={data?.Length ?? 0}, error={error?.LocalizedDescription ?? "none"}");
                    
                    if (error != null)
                    {
                        tcs.TrySetException(new Exception($"PDF generation failed: {error.LocalizedDescription}"));
                    }
                    else if (data != null && data.Length > 0)
                    {
                        Console.WriteLine($"PdfGenerator: Success! {data.Length} bytes");
                        tcs.TrySetResult(data.ToArray());
                    }
                    else
                    {
                        tcs.TrySetException(new Exception("PDF generation returned no data"));
                    }
                    
                    webView.Dispose();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PdfGenerator: Exception - {ex.Message}");
                tcs.TrySetException(ex);
            }
        });

        return await tcs.Task;
    }

    private class PdfNavigationDelegate : WKNavigationDelegate
    {
        private readonly Action _onFinished;
        private readonly Action<string>? _onError;

        public PdfNavigationDelegate(Action onFinished, Action<string>? onError = null)
        {
            _onFinished = onFinished;
            _onError = onError;
        }

        public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
        {
            _onFinished?.Invoke();
        }

        public override void DidFailNavigation(WKWebView webView, WKNavigation navigation, NSError error)
        {
            _onError?.Invoke(error.LocalizedDescription);
            _onFinished?.Invoke();
        }
        
        public override void DidFailProvisionalNavigation(WKWebView webView, WKNavigation navigation, NSError error)
        {
            _onError?.Invoke(error.LocalizedDescription);
            _onFinished?.Invoke();
        }
    }
}
