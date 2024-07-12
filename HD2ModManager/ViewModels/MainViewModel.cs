using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HD2ModManager.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace HD2ModManager.ViewModels
{
	public sealed partial class MainViewModel : ObservableObject
	{
		public ObservableCollection<ModViewModel> Mods { get; }

		private readonly HD2ModManagerLib.HD2ModManager _manager;
		private readonly IServiceProvider _provider;
		[ObservableProperty]
		private Visibility _workingVisibility = Visibility.Hidden;
		[ObservableProperty]
		private string _workText = string.Empty;

		public MainViewModel(IServiceProvider provider, HD2ModManagerLib.HD2ModManager manager)
		{
			_provider = provider;
			_manager = manager;
			Mods = new(_manager.Mods.Select(m => new ModViewModel(this, _manager, m)).ToArray());
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

			var success = await Task.Run(() => _manager.AddMod(fileDilog.FileName));

			WorkingVisibility = Visibility.Hidden;

			if (!success)
			{
				MessageBox.Show(App.Current.MainWindow, "Error adding mod!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			Mods.Add(new ModViewModel(this, _manager, _manager.Mods.Last()));
			_manager.Save();
		}

		[RelayCommand]
		void Save()
		{
			try
			{
				_manager.Save();
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

				await Task.Run(() => _manager.PurgeMods());

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
				
				await Task.Run(_manager.InstallMods);
				
				WorkingVisibility = Visibility.Hidden;

				MessageBox.Show(App.Current.MainWindow, "Mods installed.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch(Exception ex)
			{
				MessageBox.Show(App.Current.MainWindow, "Error installing mods!\n\nException:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		void ShowLog()
		{
			_provider.GetService<LogWindow>()?.Show();
		}
	}
}
