namespace JournalAppBlazor;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		
		// Global exception handlers
		AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
		{
			var ex = args.ExceptionObject as Exception;
			Console.WriteLine("=== UNHANDLED EXCEPTION ===");
			Console.WriteLine($"Message: {ex?.Message}");
			Console.WriteLine($"Stack: {ex?.StackTrace}");
			Console.WriteLine($"Inner: {ex?.InnerException?.Message}");
		};
		
		TaskScheduler.UnobservedTaskException += (sender, args) =>
		{
			Console.WriteLine("=== UNOBSERVED TASK EXCEPTION ===");
			Console.WriteLine($"Message: {args.Exception?.Message}");
			Console.WriteLine($"Stack: {args.Exception?.StackTrace}");
			args.SetObserved(); // Prevent crash
		};
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new MainPage()) { Title = "JournalAppBlazor" };
	}
}
