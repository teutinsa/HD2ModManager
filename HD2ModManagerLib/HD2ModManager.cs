// Ignore Spelling: HD

using Microsoft.Extensions.Logging;
using SharpCompress.Archives;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace HD2ModManagerLib;

public sealed partial class HD2ModManager
{
	private static readonly DirectoryInfo s_appDataDir = new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(HD2ModManager)));
	private static readonly DirectoryInfo s_modsDir = new(Path.Combine(s_appDataDir.FullName, "Mods"));
	private static readonly FileInfo s_enabledJson = new(Path.Combine(s_appDataDir.FullName, "enabled.json"));
	private static readonly FileInfo s_installedJson = new(Path.Combine(s_appDataDir.FullName, "installed.json"));
	private static readonly DirectoryInfo s_tmpDir = new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", nameof(HD2ModManager)));
	private static readonly DirectoryInfo s_stageDir = new(Path.Combine(s_tmpDir.FullName, "Staging"));
	private readonly DirectoryInfo _hd2Dir;
	private readonly DirectoryInfo _binDir;
	private readonly DirectoryInfo _dataDir;
	private readonly DirectoryInfo _toolsDir;
	private readonly FileInfo _exeFile;
	private ILogger<HD2ModManager>? _logger;

	public IReadOnlyList<Mod> Mods => _mods;

	private readonly List<Mod> _mods;

	public HD2ModManager(string hd2InstallPath, ILogger<HD2ModManager>? logger = null)
		: this(new DirectoryInfo(hd2InstallPath), logger)
	{ }

	public HD2ModManager(DirectoryInfo hd2InstallDir, ILogger<HD2ModManager>? logger = null)
	{
		_logger = logger;
		_logger?.LogInformation("Initializing manager...");
		
		_hd2Dir = hd2InstallDir;

		_logger?.LogInformation("Validating given Helldivers 2 installation directory...");
		if (!_hd2Dir.Exists)
		{
			var ex = new ArgumentException("Given Helldivers 2 installation directory does not exists!", nameof(hd2InstallDir));
			_logger?.LogCritical(ex, "Failed!");
			throw ex;
		}

		_binDir = _hd2Dir.EnumerateDirectories().FirstOrDefault(static di => di.Name == "bin")!;
		if (_binDir is null)
		{
			var ex = new ArgumentException("Given Helldivers 2 installation directory does not contain a directory named \"bin\"!", nameof(hd2InstallDir));
			_logger?.LogCritical(ex, "Failed!");
			throw ex;
		}
		_dataDir = _hd2Dir.EnumerateDirectories().FirstOrDefault(static di => di.Name == "data")!;
		if (_dataDir is null)
		{
			var ex = new ArgumentException("Given Helldivers 2 installation directory does not contain a directory named \"data\"!", nameof(hd2InstallDir));
			_logger?.LogCritical(ex, "Failed!");
			throw ex;
		}
		_toolsDir = _hd2Dir.EnumerateDirectories().FirstOrDefault(static di => di.Name == "tools")!;
		if (_toolsDir is null)
		{
			var ex = new ArgumentException("Given Helldivers 2 installation directory does not contain a directory named \"tools\"!", nameof(hd2InstallDir));
			_logger?.LogCritical(ex, "Failed!");
			throw ex;
		}

		_exeFile = _binDir.EnumerateFiles().FirstOrDefault(static fi => fi.Name == "helldivers2.exe")!;
		if (_exeFile is null)
		{
			var ex = new ArgumentException("Given Helldivers 2 binary directory does not contain a file named \"helldivers2.exe\"!", nameof(hd2InstallDir));
			_logger?.LogCritical(ex, "Failed!");
			throw ex;
		}

		if (!s_appDataDir.Exists)
			s_appDataDir.Create();

		if (!s_modsDir.Exists)
			s_modsDir.Create();

		_mods = [];

		foreach(var dir in s_modsDir.EnumerateDirectories())
		{
			var manifest = ModData.Deserialize(Path.Combine(dir.FullName, "manifest.json"));
			_mods.Add(new Mod(manifest, dir));
		}

		if (s_enabledJson.Exists)
		{
			using var stream = s_enabledJson.OpenRead();
			var enableds = JsonSerializer.Deserialize<ImmutableDictionary<Guid, int>>(stream);
			foreach (var e in enableds!)
				if (GetModByGuid(e.Key) is Mod mod)
				{
					mod.Enabled = true;
					mod.Option = e.Value;
				}
		}

		_logger?.LogInformation("Manager initialization successful.");
	}

	public bool AddMod(string zipFilePath, bool enable = true)
	{
		return AddMod(new FileInfo(zipFilePath), enable);
	}

	public bool AddMod(FileInfo zipFile, bool enable = true)
	{
		_logger?.LogInformation("Attempting to add mod from \"{FileName}\"...", zipFile.FullName);

		using var archive = ArchiveFactory.Open(zipFile.FullName);
		var manifestEntry = archive.Entries.FirstOrDefault(static e => e.Key == "manifest.json");

		ModData manifest;
		if (manifestEntry is null)
		{
			_logger?.LogWarning("No \"manifest.json\" found in \"{FileName}\"! The mod manager will attempt to infer the structure form the archive layout.", zipFile.FullName);

			if (!archive.Entries.Any(static e => e.Key is string str && GetPatchRegex().IsMatch(str)))
			{
				_logger?.LogError("No patch files found in archive!");
				return false;
			}

			var entryCount = archive.Entries.Count();
			_logger?.LogDebug("Found {count} archive entries.", entryCount);

			var hasSubDir = archive.Entries.Any(static e => e.Key is string str && str.Contains('\\'));
			_logger?.LogDebug("Has sub-directories: {value}", hasSubDir);

			var only1SubDir = archive.Entries.All(static e => e.Key is string str && str.Count(static c => c == '\\' || c == '/') == 1);
			_logger?.LogDebug("Has only one sub-directory: {value}", only1SubDir);

			string[]? options;
			if (entryCount % 3 == 0 && !hasSubDir)
			{
				options = null;
				_logger?.LogInformation("No sub-directory found zero options inferred.");
			}
			else if (entryCount > 3 && only1SubDir)
			{
				_logger?.LogInformation("Found sub-directories attempting to infer options...");

				var directories = archive.Entries.Where(static e => e.IsDirectory).ToArray();
				var directoryCount = directories.Length;
				var patchCount = archive.Entries.Count(static e => e.Key is string str && str.Contains("patch_0"));
				//var eachHas = (patchCount / directoryCount) == 3;
				var devisable = (patchCount % 3) == 0;

				if (!devisable)
				{
					_logger?.LogError("Not every sub-directory contains a multiple of 3 patch files!");
					return false;
				}

				options = directories.Select(e => e.Key!).ToArray();
				_logger?.LogInformation("");
			}
			else
			{
				_logger?.LogError("Failed to infer mod options!");
				return false;
			}

			manifest = new()
			{
				Guid = Guid.NewGuid(),
				Name = zipFile.Name[..^zipFile.Extension.Length],
				Description = "Locally imported mod.",
				Options = options
			};
			_logger?.LogInformation("Successfully inferred manifest from archive.");
		}
		else
		{
			_logger?.LogInformation("Found \"mainfest.json\" in archive.");
			_logger?.LogInformation("Reading \"mainfest.json\" from archive...");
			using var stream = manifestEntry.OpenEntryStream();
			manifest = ModData.Deserialize(stream);
			_logger?.LogInformation("Successfully read manifest.");
		}

		var modDir = new DirectoryInfo(Path.Combine(s_modsDir.FullName, zipFile.Name[..^zipFile.Extension.Length]));
		if (modDir.Exists)
			return false;

		modDir.Create();
		archive.ExtractToDirectory(modDir.FullName);
		manifest.Serialize(new FileInfo(Path.Combine(modDir.FullName, "manifest.json")));

		var mod = new Mod(manifest, modDir)
		{
			Enabled = enable
		};
		_mods.Add(mod);

		_logger?.LogInformation("Mod \"{ModName}\" added successfully.", mod.Name);
		return true;
	}

	public void RemoveMod(Mod mod)
	{
		_logger?.LogInformation("Removing mod \"{ModName}\"...", mod.Name);
		if (_mods.Remove(mod))
			Directory.Delete(mod.ModDir.FullName, true);
		_logger?.LogInformation("Removed mod \"{ModName}\" successfully.", mod.Name);
	}

	public void Save()
	{
		_logger?.LogInformation("Saving mod configuration...");
		using var stream = s_enabledJson.Create();
		JsonSerializer.Serialize(stream, _mods.Where(static m => m.Enabled).Select(static m => new KeyValuePair<Guid, int>(m._data.Guid, m.Option)).ToDictionary());
		_logger?.LogInformation("Configuration saved successfully.");
	}

	public void PurgeMods(bool indiscriminate = false)
	{
		if (indiscriminate)
		{
			_logger?.LogInformation("Purging mods indiscriminately...");

			foreach (var file in _dataDir.EnumerateFiles("*patch_*"))
				file.Delete();
		}
		else
		{
			_logger?.LogInformation("Purging mods...");

			if (!s_installedJson.Exists)
				return;
			using var stream = s_installedJson.OpenRead();
			var files = JsonSerializer.Deserialize<string[]>(stream);
			foreach (var file in files!)
				File.Delete(Path.Combine(_dataDir.FullName, file));
		}

		_logger?.LogInformation("Purge complete.");
	}

	public bool InstallMods()
	{
		_logger?.LogInformation("Installing mods...");

		Save();
		PurgeMods();
		if (s_stageDir.Exists)
			s_stageDir.Delete(true);
		s_stageDir.Create();

		var mods = _mods.Where(static m => m.Enabled).ToArray();
		_logger?.LogInformation("Found {count} enabled mods.", mods.Length);

		if (mods.Length > 0)
		{
			var modFiles = mods.SelectMany(static m =>
			{
				DirectoryInfo dir;
				if (m.Options is not null)
					dir = new(Path.Combine(m.ModDir.FullName, m.Options[m.Option]));
				else
					dir = m.ModDir;
				return dir.EnumerateFiles("*patch_*");
			}).ToArray().AsSpan();
			_logger?.LogInformation("Found {count} patch files.", modFiles.Length);

			var grouped = new Dictionary<string, List<FileInfo>>();
			foreach (var file in modFiles)
			{
				var name = file.Name[0..16];
				if (!grouped.ContainsKey(name))
					grouped.Add(name, []);
				grouped[name].Add(file);
			}
			_logger?.LogDebug("Grouped files into {count} groups.", grouped.Count);

			_logger?.LogInformation("Beginning renaming and copying sequence...");
			foreach (var (_, files) in grouped)
				for (int i = 0; i < files.Count; i++)
				{
					var file = files[i];
					var patch = i / 3;
					var match = GetPatchRegex().Match(file.Name);
					var name = file.Name.Replace(match.Value, $"patch_{patch}");
					file.CopyTo(Path.Combine(s_stageDir.FullName, name));
					_logger?.LogDebug("Copied file \"{name}\" as \"{newName}\".", file.Name, name);
				}
			_logger?.LogInformation("Renaming and copying successful.");

			foreach (var file in s_stageDir.EnumerateFiles())
				file.CopyTo(Path.Combine(_dataDir.FullName, file.Name));
		}
		else
			_logger?.LogInformation("Skipping installation.");

		var stagedFiles = s_stageDir.EnumerateFiles().Select(static f => f.Name).ToArray();
		using var stream = s_installedJson.Create();
		JsonSerializer.Serialize(stream, stagedFiles);
		_logger?.LogDebug("Saved the list of deployed files.");

		s_stageDir.Delete(true);

		_logger?.LogInformation("Mods installed successfully.");
		return true;
	}

	public Mod? GetModByGuid(Guid guid)
	{
		return _mods.Where(m => m._data.Guid == guid).FirstOrDefault();
	}

	/// <inheritdoc/>
	public override string ToString()
	{
		return $"HD2ModManager\n{{\n\tAppDataDir = {s_appDataDir},\n\tTempDir = {s_tmpDir},\n\tHD2Dir = {_hd2Dir},\n\tBinDir = {_binDir},\n\tDataDir = {_dataDir},\n\tToolsDir = {_toolsDir},\n\tExeFile = {_exeFile}\n}}";
	}

	[GeneratedRegex(@"(?!\.)patch_[0-9]+(?=\.?)")]
	private static partial Regex GetPatchRegex();
}
