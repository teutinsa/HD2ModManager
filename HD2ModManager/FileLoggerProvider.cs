// Ignore Spelling: App

using Microsoft.Extensions.Logging;

namespace HD2ModManager;

public sealed class FileLoggerProvider : ILoggerProvider
{
	public ILogger CreateLogger(string categoryName)
	{
		return new FileLogger(categoryName);
	}

	public void Dispose()
	{ }
}

