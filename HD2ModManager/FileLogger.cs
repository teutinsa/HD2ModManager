// Ignore Spelling: App

using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;

namespace HD2ModManager;

public sealed class FileLogger : ILogger
{
	private readonly string _name;

	public FileLogger(string name)
	{
		_name = name + ".log";
		if (File.Exists(_name))
			File.Delete(_name);
	}

	public IDisposable? BeginScope<TState>(TState state) where TState : notnull
	{
		return null;
	}

	public bool IsEnabled(LogLevel logLevel)
	{
		return logLevel != LogLevel.None;
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
		builder.AppendLine();

		if (exception is not null)
		{
			builder.Append('\t');
			builder.Append(exception.GetType().Name);
			builder.Append(": ");
			builder.Append(exception.Message);
			builder.AppendLine();
			if (exception.StackTrace is not null)
			{
				builder.Append("\t\t");
				builder.Append(exception.StackTrace?.ReplaceLineEndings($"{Environment.NewLine}\t\t"));
				builder.AppendLine();
			}
		}

		File.AppendAllText(_name, builder.ToString());
	}
}

