using Microsoft.JSInterop;

namespace JournalAppBlazor.Services;

public interface IThemeService
{
    event Action? OnThemeChanged;
    string CurrentTheme { get; }
    Task SetThemeAsync(string theme);
    Task InitializeThemeAsync();
}

public class ThemeService : IThemeService
{
    private readonly IJSRuntime _jsRuntime;
    private const string ThemeKey = "journal_app_theme";
    private const string DefaultTheme = "light";
    private bool _isInitialized = false;
    
    public event Action? OnThemeChanged;
    public string CurrentTheme { get; private set; } = DefaultTheme;

    public ThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeThemeAsync()
    {
        if (_isInitialized) return;
        
        try
        {
            // In MAUI, we can use Preferences for persistence
            CurrentTheme = Preferences.Get(ThemeKey, DefaultTheme);
            
            // Small delay to ensure Blazor is fully ready
            await Task.Delay(100);
            
            await ApplyThemeAsync(CurrentTheme);
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ThemeService init error: {ex.Message}");
            _isInitialized = true; // Mark as initialized anyway to avoid loops
        }
    }

    public async Task SetThemeAsync(string theme)
    {
        if (theme != "light" && theme != "dark")
            theme = DefaultTheme;

        CurrentTheme = theme;
        Preferences.Set(ThemeKey, theme);
        await ApplyThemeAsync(theme);
        OnThemeChanged?.Invoke();
    }

    private async Task ApplyThemeAsync(string theme)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("applyTheme", theme);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ApplyTheme error: {ex.Message}");
            // Fallback if JS interop fails - that's okay
        }
    }
}
