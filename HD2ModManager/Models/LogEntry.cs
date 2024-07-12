using Microsoft.Extensions.Logging;

namespace HD2ModManager.Models;

public sealed class LogEntry
{
	public required LogLevel Level { init; get; }

	public required string Message { init; get; }
}
