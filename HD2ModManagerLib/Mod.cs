// Ignore Spelling: HD

using System.Collections.Generic;
using System.IO;

namespace HD2ModManagerLib;

public sealed class Mod
{
	public string Name => _data.Name;

	public string Description => _data.Description;

	public string? IconPath => _data.IconPath;

	public IReadOnlyList<string>? Options => _data.Options;

	public bool Enabled { get; set; }

	public int Option { get; set; }

	public DirectoryInfo ModDir { get; }

	internal readonly ModData _data;

	internal Mod(ModData data, DirectoryInfo modDir)
	{
		_data = data;

		ModDir = modDir;
	}
}
