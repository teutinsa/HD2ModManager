using CommunityToolkit.Mvvm.ComponentModel;
using HD2ModManager.Models;
using Microsoft.Extensions.Logging;
using System.Windows.Media;

namespace HD2ModManager.ViewModels;

public sealed class LogEntryViewModel(LogEntry entry) : ObservableObject
{
	public Brush Color
	{
		get
		{
			switch (_entry.Level)
			{
				case LogLevel.Trace:
					return new SolidColorBrush(Colors.Gray);
					
				case LogLevel.Debug:
					return new SolidColorBrush(Colors.White);

				case LogLevel.Information:
					return new SolidColorBrush(Colors.CornflowerBlue);

				case LogLevel.Warning:
					return new SolidColorBrush(Colors.Yellow);

				case LogLevel.Error:
					return new SolidColorBrush(Colors.Red);

				case LogLevel.Critical:
					return new SolidColorBrush(Colors.DarkRed);

				default:
					return new SolidColorBrush(Colors.White);
			}
		}
	}

	public string Message => _entry.Message;

	private readonly LogEntry _entry = entry;
}
