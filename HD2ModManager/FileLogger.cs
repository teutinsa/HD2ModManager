// Ignore Spelling: App

using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;

namespace HD2ModManager;

public sealed class FileLogger(string name, StreamWriter stream) : ILogger
{
	private readonly string _name = name;
	private readonly StreamWriter _stream = stream;

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
		builder.Append(_name);
		builder.Append(" -> ");
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

		_stream.WriteLine(builder.ToString());
		_stream.Flush();
	}
}

