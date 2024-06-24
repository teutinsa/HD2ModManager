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

		public MainViewModel()
		{
			Mods = new(App.Current.Manager.Mods.Select(m => new ModViewModel(this, m)).ToArray());
		}

		[RelayCommand]
		void Add()
		{
			var fileDilog = new OpenFileDialog
			{
				CheckFileExists = true,
				CheckPathExists = true,
				DefaultDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
				Filter = "Archives (*.zip,*.rar)|*.zip;*.rar",
				Multiselect = false,
			};

			var result = fileDilog.ShowDialog(App.Current.MainWindow);
			if (!result.HasValue || !result.Value)
				return;

			if (!App.Current.Manager.AddMod(fileDilog.FileName))
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
		void Purge()
		{
			try
			{
				App.Current.Manager.PurgeMods();
				MessageBox.Show(App.Current.MainWindow, "Mods purged.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch(Exception ex)
			{
				MessageBox.Show(App.Current.MainWindow, "Error purging mods!\n\nException:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		void Deploy()
		{
			try
			{
				App.Current.Manager.InstallMods();
				MessageBox.Show(App.Current.MainWindow, "Mods installed.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch(Exception ex)
			{
				MessageBox.Show(App.Current.MainWindow, "Error installing mods!\n\nException:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}
