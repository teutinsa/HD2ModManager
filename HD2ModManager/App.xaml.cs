using Microsoft.Win32;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace HD2ModManager;

public partial class App : Application
{
	public static new App Current => (App)Application.Current;

	[AllowNull]
	public HD2ModManagerLib.HD2ModManager Manager { get; private set; }

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

		try
		{
			Manager = new(Settings.Default.HD2Path);
		}
		catch(Exception ex)
		{
			MessageBox.Show("Invalid Helldivers 2 path!\n\nException:\n" + ex.Message + "\n\n" + Settings.Default.HD2Path, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			Settings.Default.Reset();
			Shutdown();
			return;
		}
		
		MainWindow = new MainWindow();
		MainWindow.Show();
	}
}

