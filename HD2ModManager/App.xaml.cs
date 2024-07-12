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

	public App()
	{
		var builder = new HostBuilder();
		builder.ConfigureLogging(static log =>
		{
			log.AddProvider(new EntryLoggerProvider());
#if DEBUG
			log.AddDebug();
#endif
#if RELEASE
			log.AddProvider(new FileLoggerProvider());
#endif
			log.SetMinimumLevel(LogLevel.Trace);
		});
		builder.ConfigureServices(static services =>
		{
			services.AddSingleton<HD2ModManagerLib.HD2ModManager>(static provider => new(Settings.Default.HD2Path, provider.GetService<ILogger<HD2ModManagerLib.HD2ModManager>>()));

			services.AddSingleton<MainWindow>();
			services.AddSingleton<LogWindow>();

			services.AddTransient<MainViewModel>();
			services.AddTransient<LogViewModel>();
		});

		Host = builder.Build();
	}

	protected override void OnStartup(StartupEventArgs e)
	{

		if (string.IsNullOrEmpty(Settings.Default.HD2Path))
		{
			var dialog = new OpenFolderDialog
			{
				Multiselect = false,
				Title = "Please select your Helldivers 2 install path"
			};

			var result = dialog.ShowDialog();
			if (!result.HasValue || !result.Value)
			{
				MessageBox.Show("Setup canceled by user.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				Shutdown();
				return;
			}

			Settings.Default.HD2Path = dialog.FolderName;
			Settings.Default.Save();
		}

		MainWindow = Host.Services.GetRequiredService<MainWindow>();
		MainWindow.Show();
	}

	protected override void OnExit(ExitEventArgs e)
	{
		Host.Services.GetRequiredService<HD2ModManagerLib.HD2ModManager>().Save();
		base.OnExit(e);
	}
}

