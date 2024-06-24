using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HD2ModManagerLib;
using System.Windows;
using System.Windows.Controls;

namespace HD2ModManager.ViewModels
{
	internal sealed partial class ModViewModel(MainViewModel viewModel, Mod mod) : ObservableObject
	{
		public string IconPath => _mod.IconPath ?? "Resources/Images/icon.png";

		public string Name => _mod.Name;

		public string Description => _mod.Description;

		public bool Enabled
		{
			get => _mod.Enabled;
			set
			{
				_mod.Enabled = value;
				OnPropertyChanged();
			}
		}

		public Visibility OptionsVisibility => _mod.Options is not null ? Visibility.Visible : Visibility.Collapsed;

		public int SelectedOption
		{
			get => _mod.Option;
			set
			{
				_mod.Option = value;
				OnPropertyChanged();
			}
		}

		public IReadOnlyList<string> Options => _mod.Options ?? [];

		private readonly MainViewModel _viewModel = viewModel;
		private readonly Mod _mod = mod;

		[RelayCommand]
		void Remove()
		{
			App.Current.Manager.RemoveMod(_mod);
			_viewModel.Mods.Remove(this);
		}
	}
}
