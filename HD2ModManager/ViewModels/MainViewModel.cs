using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace HD2ModManager.ViewModels
{
	internal sealed partial class MainViewModel : ObservableObject
	{
		public ObservableCollection<ModViewModel> Mods { get; }

		[ObservableProperty]
		private Visibility _workingVisibility = Visibility.Hidden;
		[ObservableProperty]
		private string _workText = string.Empty;

		public MainViewModel()
		{
			Mods = new(App.Current.Manager.Mods.Select(m => new ModViewModel(this, m)).ToArray());
		}

		[RelayCommand]
		async Task Add()
		{
			var fileDilog = new OpenFileDialog
			{
				Title = "Add mod archive...",
				CheckFileExists = true,
				CheckPathExists = true,
				InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
				Filter = "Archives (*.zip,*.rar,*.7z)|*.zip;*.rar;*.7z|All Files (*.*)|*.*",
				Multiselect = false,
			};

			var result = fileDilog.ShowDialog(App.Current.MainWindow);
			if (!result.HasValue || !result.Value)
				return;

			WorkText = "Adding mod...";
			WorkingVisibility = Visibility.Visible;

			var success = await Task.Run(() => App.Current.Manager.AddMod(fileDilog.FileName));

			WorkingVisibility = Visibility.Hidden;

			if (!success)
			{
				MessageBox.Show(App.Current.MainWindow, "Error adding mod!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			Mods.Add(new ModViewModel(this, App.Current.Manager.Mods.Last()));
			App.Current.Manager.Save();
		}

		[RelayCommand]
		void Save()
		{
			try
			{
				App.Current.Manager.Save();
			}
			catch(Exception ex)
			{
				MessageBox.Show(App.Current.MainWindow, "Error saving config!\n\nException:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		async Task Purge()
		{
			try
			{
				WorkText = "Purging mods...";
				WorkingVisibility = Visibility.Visible;

				await Task.Run(static () => App.Current.Manager.PurgeMods());

				WorkingVisibility = Visibility.Hidden;

				MessageBox.Show(App.Current.MainWindow, "Mods purged.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch(Exception ex)
			{
				MessageBox.Show(App.Current.MainWindow, "Error purging mods!\n\nException:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		async Task Deploy()
		{
			try
			{
				WorkText = "Deploying mods...";
				WorkingVisibility = Visibility.Visible;
				
				await Task.Run(App.Current.Manager.InstallMods);
				
				WorkingVisibility = Visibility.Hidden;

				MessageBox.Show(App.Current.MainWindow, "Mods installed.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch(Exception ex)
			{
				MessageBox.Show(App.Current.MainWindow, "Error installing mods!\n\nException:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}
