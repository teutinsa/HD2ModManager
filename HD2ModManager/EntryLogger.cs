// Ignore Spelling: App

using HD2ModManager.Models;
using Microsoft.Extensions.Logging;
using System.Text;

namespace HD2ModManager;

public sealed class EntryLogger() : ILogger
{
	public static IReadOnlyList<LogEntry> Entries => s_entries;

	public static event EventHandler<LogEntry>? EntryLogged;

	private static readonly List<LogEntry> s_entries = [];

	public IDisposable? BeginScope<TState>(TState state) where TState : notnull
	{
		return null;
	}

	public bool IsEnabled(LogLevel logLevel)
	{
		return logLevel >= LogLevel.Information;
	}

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		if (!IsEnabled(logLevel))
			return;

		ArgumentNullException.ThrowIfNull(formatter);

		string message = formatter.Invoke(state, exception);
		if (string.IsNullOrEmpty(message))
			return;

		var builder = new StringBuilder();
		builder.Append('[');
		builder.Append(DateTime.Now.ToString("HH:mm::ss"));
		builder.Append("] ");
		builder.Append(logLevel.ToString());
		builder.Append(": ");
		builder.Append(message);

		if (exception is not null)
		{
			builder.AppendLine();
			builder.Append('\t');
			builder.Append(exception.GetType().Name);
			builder.Append(": ");
			builder.Append(exception.Message);
			if (exception.StackTrace is not null)
			{
				builder.AppendLine();
				builder.Append("\t\t");
				builder.Append(exception.StackTrace?.ReplaceLineEndings($"{Environment.NewLine}\t\t"));
			}
		}

		var entry = new LogEntry
		{
			Level = logLevel,
			Message = builder.ToString()
		};
		s_entries.Add(entry);
		OnEntryLoggerd(entry);
	}

	internal void OnEntryLoggerd(LogEntry entry)
	{
		EntryLogged?.Invoke(this, entry);
	}
}
