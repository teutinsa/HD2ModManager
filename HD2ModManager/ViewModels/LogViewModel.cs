using CommunityToolkit.Mvvm.ComponentModel;
using HD2ModManager.Models;

namespace HD2ModManager.ViewModels;

public sealed class LogViewModel : ObservableObject
{
	public IReadOnlyList<LogEntryViewModel> Entries => EntryLogger.Entries.Select(static e => new LogEntryViewModel(e)).ToArray();

	public LogViewModel()
	{
		EntryLogger.EntryLogged += Logger_EntryLogged;
	}

	private void Logger_EntryLogged(object? sender, LogEntry e)
	{
		OnPropertyChanged(nameof(Entries));
	}
}
