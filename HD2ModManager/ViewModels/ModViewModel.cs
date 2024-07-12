using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HD2ModManagerLib;
using System.Windows;

namespace HD2ModManager.ViewModels
{
	public sealed partial class ModViewModel(MainViewModel viewModel, HD2ModManagerLib.HD2ModManager manager, Mod mod) : ObservableObject
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
		private readonly HD2ModManagerLib.HD2ModManager _manager = manager;
		private readonly Mod _mod = mod;

		[RelayCommand]
		void Remove()
		{
			_manager.RemoveMod(_mod);
			_viewModel.Mods.Remove(this);
		}
	}
}
