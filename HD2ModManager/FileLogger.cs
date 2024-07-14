// Ignore Spelling: App

using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;

namespace HD2ModManager;

public sealed class FileLogger : ILogger, IDisposable
{
	private readonly string _name;
	private readonly FileStream _fileStream;
	private readonly StreamWriter _stream;

	public FileLogger(string name)
	{
		_name = name;
		_fileStream = new FileStream("HD2ModManager.log", FileMode.Create, FileAccess.Write, FileShare.Read);
		_stream = new StreamWriter(_fileStream);
	}

	public void Dispose()
	{
		_stream.Dispose();
		_fileStream.Dispose();
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
		builder.Append(_name);
		builder.Append(" -> ");
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

		_stream.WriteLine(builder.ToString());
		_stream.Flush();
	}
}

