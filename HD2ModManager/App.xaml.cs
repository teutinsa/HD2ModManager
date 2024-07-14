// Ignore Spelling: App

using HD2ModManager.ViewModels;
using HD2ModManager.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System.Windows;

namespace HD2ModManager;

public partial class App : Application
{
	public static new App Current => (App)Application.Current;

	public IHost Host { get; }

	private readonly ILogger<App> _logger;

	public App()
	{
		DispatcherUnhandledException += App_DispatcherUnhandledException;

		var builder = new HostBuilder();
		builder.ConfigureLogging(static log =>
		{
#if DEBUG
			log.AddDebug();
#endif
#if RELEASE
			log.AddProvider(new FileLoggerProvider());
#endif
			log.AddProvider(new EntryLoggerProvider());
			log.SetMinimumLevel(LogLevel.Trace);
		});
		builder.ConfigureServices(static services =>
		{
			services.AddSingleton<HD2ModManagerLib.HD2ModManager>(static provider => new(Settings.Default.HD2Path, provider.GetRequiredService<ILogger<HD2ModManagerLib.HD2ModManager>>()));

			services.AddSingleton<MainWindow>();
			services.AddSingleton<LogWindow>();

			services.AddTransient<MainViewModel>();
			services.AddTransient<LogViewModel>();
		});

		Host = builder.Build();

		_logger = Host.Services.GetRequiredService<ILogger<App>>();
	}

	protected override void OnStartup(StartupEventArgs e)
	{
		_logger.LogInformation("Starting application.");

		_logger.LogInformation("Checking if for Helldivers 2 path in user settings...");
		if (string.IsNullOrEmpty(Settings.Default.HD2Path) || string.IsNullOrWhiteSpace(Settings.Default.HD2Path))
		{
			_logger.LogInformation("Helldivers 2 path in user settings is wither empty or white space. Prompting user for path...");
			var dialog = new OpenFolderDialog
			{
				Multiselect = false,
				Title = "Please select your Helldivers 2 install path"
			};

			var result = dialog.ShowDialog();
			if (!result.HasValue || !result.Value)
			{
				_logger.LogInformation("Setup canceled by user.");
				MessageBox.Show("Setup canceled by user.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				Shutdown();
				return;
			}

			_logger.LogInformation("Possible helldivers 2 path received. Validation will occur later.");

			Settings.Default.HD2Path = dialog.FolderName;
			Settings.Default.Save();
		}
		else
			_logger.LogInformation("Found Helldivers 2 path in user settings.");

		_logger.LogDebug("Initializing main window.");
		MainWindow = Host.Services.GetRequiredService<MainWindow>();
		MainWindow.Closed += MainWindow_Closed;
		MainWindow.Show();
		_logger.LogDebug("Showing main window");
	}

	protected override void OnExit(ExitEventArgs e)
	{
		_logger.LogInformation("Quitting application.");

		Host.Services.GetRequiredService<HD2ModManagerLib.HD2ModManager>().Save();
		
		base.OnExit(e);
	}

	private void MainWindow_Closed(object? sender, EventArgs e)
	{
		_logger.LogInformation("Main window closed. Shutting down.");

		Shutdown();
	}

	private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
	{
		_logger.LogCritical(e.Exception, "An unhandled exception occurred!");

		var result = MessageBox.Show("An unhandled exception was thrown! This can be because of invalid settings.\n\nDo you want to reset your settings? This may potentially fix the error.", "Uncaught exception", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes;
		if (result)
		{
			Settings.Default.Reset();
			Settings.Default.Save();
		}
	}
}

