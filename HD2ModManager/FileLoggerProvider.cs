// Ignore Spelling: App

using Microsoft.Extensions.Logging;
using System.IO;

namespace HD2ModManager;

public sealed class FileLoggerProvider : ILoggerProvider
{
	private readonly FileStream _fileStream;
	private readonly StreamWriter _stream;

	public FileLoggerProvider()
	{

		_fileStream = new FileStream("HD2ModManager.log", FileMode.Create, FileAccess.Write, FileShare.Read);
		_stream = new StreamWriter(_fileStream);
	}

	public ILogger CreateLogger(string categoryName)
	{
		return new FileLogger(categoryName, _stream);
	}

	public void Dispose()
	{
		_stream.Dispose();
		_fileStream.Dispose();
	}
}

