// Ignore Spelling: HD

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text.Json;

namespace HD2ModManagerLib;

public sealed record ModData
{
	public required Guid Guid { get; init; }

	public required string Name { get; init; }

	public required string Description { get; init; }

	public string? IconPath { get; init; }

	public string[]? Options { get; init; }

	private static JsonSerializerOptions _options = new()
	{
		AllowTrailingCommas = true,
		WriteIndented = true,
	};

	public static ModData Deserialize(string filePath)
	{
		return Deserialize(new FileInfo(filePath));
	}

	public static ModData Deserialize(FileInfo file)
	{
		using var stream = file.OpenRead();
		return Deserialize(stream);
	}

	public static ModData Deserialize(Stream uf8Stream)
	{
		return JsonSerializer.Deserialize<ModData>(uf8Stream, _options) ?? throw new SerializationException();
	}

	public void Serialize(FileInfo file)
	{
		using var stream = file.OpenWrite();
		JsonSerializer.Serialize(stream, this, _options);
	}
}
