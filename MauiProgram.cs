using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using JournalAppBlazor.Data;
using JournalAppBlazor.Services;

namespace JournalAppBlazor;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		try
		{
			Console.WriteLine("=== Journal App Starting ===");
			
			var builder = MauiApp.CreateBuilder();
			builder
				.UseMauiApp<App>()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				});

			builder.Services.AddMauiBlazorWebView();

#if DEBUG
			builder.Services.AddBlazorWebViewDeveloperTools();
			builder.Logging.AddDebug();
			Console.WriteLine("DEBUG: Blazor WebView Developer Tools enabled");
#endif

			// Register database context - use AddDbContextFactory for better Blazor support
			var dbPath = Path.Combine(FileSystem.AppDataDirectory, "journal.db");
			Console.WriteLine($"Database path: {dbPath}");
			
			// Use AddDbContextFactory for Blazor components
			builder.Services.AddDbContextFactory<JournalDbContext>(options =>
				options.UseSqlite($"Data Source={dbPath}")
					.EnableSensitiveDataLogging());
			
			// Also register as scoped for services
			builder.Services.AddDbContext<JournalDbContext>(options =>
				options.UseSqlite($"Data Source={dbPath}")
					.EnableSensitiveDataLogging());

		// Register services
		builder.Services.AddScoped<IJournalService, JournalService>();
		builder.Services.AddScoped<IMoodService, MoodService>();
		builder.Services.AddScoped<ITagService, TagService>();
		builder.Services.AddScoped<IStreakService, StreakService>();
		builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
		builder.Services.AddScoped<IExportService, ExportService>();
		builder.Services.AddSingleton<IThemeService, ThemeService>();
		builder.Services.AddSingleton<IAuthService, AuthService>();

			Console.WriteLine("DEBUG: Building MauiApp...");
			var app = builder.Build();
			Console.WriteLine("DEBUG: MauiApp built successfully");

			// Initialize database asynchronously - don't block the UI thread
			_ = Task.Run(async () =>
			{
				try
				{
					await Task.Delay(500); // Give Blazor time to start
					Console.WriteLine("DEBUG: Creating database scope...");
					using var scope = app.Services.CreateScope();
					var context = scope.ServiceProvider.GetRequiredService<JournalDbContext>();
					var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseInit");
					logger.LogInformation("Initializing database at: {DbPath}", dbPath);
					Console.WriteLine($"Initializing database at: {dbPath}");
					
					await DatabaseInitializer.InitializeAsync(context);
					
					logger.LogInformation("Database initialized successfully");
					Console.WriteLine("Database initialized successfully");
				}
				catch (Exception ex)
				{
					var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseInit");
					logger.LogError(ex, "Failed to initialize database: {Message}", ex.Message);
					Console.WriteLine($"ERROR: Database initialization failed: {ex.Message}");
					Console.WriteLine($"Stack: {ex.StackTrace}");
					Console.WriteLine($"Inner: {ex.InnerException?.Message}");
				}
			});

			Console.WriteLine("DEBUG: Returning MauiApp");
			return app;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"FATAL ERROR in CreateMauiApp: {ex.Message}");
			Console.WriteLine($"Stack: {ex.StackTrace}");
			Console.WriteLine($"Inner: {ex.InnerException?.Message}");
			throw; // Re-throw to see the error
		}
	}
}
