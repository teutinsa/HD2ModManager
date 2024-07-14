// Ignore Spelling: App

using HD2ModManager.Models;
using Microsoft.Extensions.Logging;

namespace HD2ModManager;

public sealed class EntryLoggerProvider : ILoggerProvider
{
	public ILogger CreateLogger(string categoryName)
	{
		return new EntryLogger(categoryName);
	}

	public void Dispose()
	{ }
}