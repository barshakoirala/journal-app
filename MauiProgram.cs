using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using JournalAppBlazor.Data;
using JournalAppBlazor.Services;
using JournalAppBlazor.Repositories;

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

			// Register repositories
			builder.Services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();
			builder.Services.AddScoped<IJournalEntryMoodRepository, JournalEntryMoodRepository>();
			builder.Services.AddScoped<IJournalEntryTagRepository, JournalEntryTagRepository>();
			builder.Services.AddScoped<IMoodRepository, MoodRepository>();
			builder.Services.AddScoped<ITagRepository, TagRepository>();
			builder.Services.AddScoped<IUserRepository, UserRepository>();
			builder.Services.AddSingleton<IUserRepositoryFactory, UserRepositoryFactory>();

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

			// Initialize database synchronously before app starts
			try
			{
				Console.WriteLine("DEBUG: Creating database scope...");
				using var scope = app.Services.CreateScope();
				var context = scope.ServiceProvider.GetRequiredService<JournalDbContext>();
				Console.WriteLine($"Initializing database at: {dbPath}");

				// Run synchronously to ensure DB is ready before UI loads
				DatabaseInitializer.InitializeAsync(context).GetAwaiter().GetResult();

				Console.WriteLine("Database initialized successfully");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"ERROR: Database initialization failed: {ex.Message}");
				Console.WriteLine($"Stack: {ex.StackTrace}");
				Console.WriteLine($"Inner: {ex.InnerException?.Message}");
				// Don't throw - let the app start and show error in UI
			}

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
