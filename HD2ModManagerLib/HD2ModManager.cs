// Ignore Spelling: HD

using SharpCompress.Archives;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace HD2ModManagerLib;

public sealed class HD2ModManager
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

	public IReadOnlyList<Mod> Mods => _mods;

	private readonly List<Mod> _mods;

	public HD2ModManager(string hd2InstallPath)
		: this(new DirectoryInfo(hd2InstallPath))
	{ }

	public HD2ModManager(DirectoryInfo hd2InstallDir)
	{
		_hd2Dir = hd2InstallDir;

		if (!_hd2Dir.Exists)
			throw new ArgumentException("Given Helldivers 2 installation directory does not exists!", nameof(hd2InstallDir));

		_binDir = _hd2Dir.EnumerateDirectories().FirstOrDefault(static di => di.Name == "bin") ?? throw new ArgumentException("Given Helldivers 2 installation directory does not contain a directory named \"bin\"!", nameof(hd2InstallDir));
		_dataDir = _hd2Dir.EnumerateDirectories().FirstOrDefault(static di => di.Name == "data") ?? throw new ArgumentException("Given Helldivers 2 installation directory does not contain a directory named \"data\"!", nameof(hd2InstallDir));
		_toolsDir = _hd2Dir.EnumerateDirectories().FirstOrDefault(static di => di.Name == "tools") ?? throw new ArgumentException("Given Helldivers 2 installation directory does not contain a directory named \"tools\"!", nameof(hd2InstallDir));
		
		_exeFile = _binDir.EnumerateFiles().FirstOrDefault(static fi => fi.Name == "helldivers2.exe") ?? throw new ArgumentException("Given Helldivers 2 binary directory does not contain a file named \"helldivers2.exe\"!", nameof(hd2InstallDir));

		if (!s_appDataDir.Exists)
			s_appDataDir.Create();

		if (!s_modsDir.Exists)
			s_modsDir.Create();

		_mods = new();

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
	}

	public bool AddMod(string zipFilePath, bool enable = true)
	{
		return AddMod(new FileInfo(zipFilePath), enable);
	}

	public bool AddMod(FileInfo zipFile, bool enable = true)
	{
		using var archive = ArchiveFactory.Open(zipFile.FullName);
		var manifestEntry = archive.Entries.FirstOrDefault(static e => e.Key == "manifest.json");

		ModData manifest;
		if (manifestEntry is null)
		{
			if (!archive.Entries.Any(static e => e.Key is string str && str.Contains("patch_0")))
				return false;

			var entryCount = archive.Entries.Count();
			var only1SubDir = archive.Entries.All(static e => e.Key is string str && str.Count(static c => c == '\\' || c == '/') <= 1);
			string[]? options;
			if (entryCount == 3)
				options = null;
			else if (entryCount > 3 && only1SubDir)
			{
				var directories = archive.Entries.Where(static e => e.IsDirectory).ToArray();
				var directoryCount = directories.Length;
				var patchCount = archive.Entries.Count(static e => e.Key is string str && str.Contains("patch_0"));
				var eachHas = patchCount / directoryCount == 3;

				if (!eachHas)
					return false;

				options = directories.Select(e => e.Key!).ToArray();
			}
			else
				return false;

			manifest = new()
			{
				Guid = Guid.NewGuid(),
				Name = zipFile.Name[..^zipFile.Extension.Length],
				Description = "Locally imported mod.",
				Options = options
			};
		}
		else
		{
			using var stream = manifestEntry.OpenEntryStream();
			manifest = ModData.Deserialize(stream);
		}

		var modDir = new DirectoryInfo(Path.Combine(s_modsDir.FullName, zipFile.Name[..^zipFile.Extension.Length]));
		if (modDir.Exists)
			return false;

		modDir.Create();
		archive.ExtractToDirectory(modDir.FullName);
		manifest.Serialize(new FileInfo(Path.Combine(modDir.FullName, "manifest.json")));

		_mods.Add(new Mod(manifest, modDir)
		{
			Enabled = enable,
		});

		return true;
	}

	public void RemoveMod(Mod mod)
	{
		if (_mods.Remove(mod))
			Directory.Delete(Path.Combine(s_modsDir.FullName, mod.Name), true);
	}

	public void Save()
	{
		using var stream = s_enabledJson.Create();
		JsonSerializer.Serialize(stream, _mods.Where(static m => m.Enabled).Select(static m => new KeyValuePair<Guid, int>(m._data.Guid, m.Option)).ToDictionary());
	}

	public void PurgeMods(bool indiscriminate = false)
	{
		if (indiscriminate)
		{
			foreach (var file in _dataDir.EnumerateFiles("*patch_*"))
				file.Delete();
		}
		else
		{
			if (!s_installedJson.Exists)
				return;
			using var stream = s_installedJson.OpenRead();
			var files = JsonSerializer.Deserialize<string[]>(stream);
			foreach (var file in files!)
				File.Delete(Path.Combine(_dataDir.FullName, file));
		}
	}

	public bool InstallMods()
	{
		Save();
		PurgeMods();
		if (s_stageDir.Exists)
			s_stageDir.Delete(true);
		s_stageDir.Create();

		var mods = _mods.Where(static m => m.Enabled).ToArray();
		if (mods.Length > 0)
		{
			var groupedMods = new Dictionary<string, List<Mod>>();
			foreach (var mod in mods)
			{
				DirectoryInfo patchFileDir;
				if (mod.Options is null)
					patchFileDir = mod.ModDir;
				else
					patchFileDir = new DirectoryInfo(Path.Combine(mod.ModDir.FullName, mod.Options[mod.Option]));

				foreach (var file in patchFileDir.EnumerateFiles("*patch_*"))
				{
					var name = file.Name.Split('.').First();
					if (!groupedMods.ContainsKey(name))
						groupedMods.Add(name, new());
					var list = groupedMods[name];
					if (!list.Contains(mod))
						list.Add(mod);
				}
			}

			foreach(var (patchName, group) in groupedMods)
				for (int i = 0; i < group.Count; i++)
				{
					var mod = group[i];

					DirectoryInfo patchFileDir;
					if (mod.Options is null)
						patchFileDir = mod.ModDir;
					else
						patchFileDir = new DirectoryInfo(Path.Combine(mod.ModDir.FullName, mod.Options[mod.Option]));

					foreach (var file in patchFileDir.EnumerateFiles("*patch_*"))
						file.CopyTo(Path.Combine(s_stageDir.FullName, file.Name.Replace("patch_0", $"patch_{i}")));
				}

			foreach (var file in s_stageDir.EnumerateFiles())
				file.CopyTo(Path.Combine(_dataDir.FullName, file.Name));
		}

		var stagedFiles = s_stageDir.EnumerateFiles().Select(static f => f.Name).ToArray();
		using var stream = s_installedJson.Create();
		JsonSerializer.Serialize(stream, stagedFiles);

		s_stageDir.Delete(true);
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
}
