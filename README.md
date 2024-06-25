# HD2ModManager
This is a simple mod manager for Helldivers 2.
It's a portable self-contained WPF application. It will take care of installing and renaming patch files as well as keeping track of them. It will also let you disable mods.

No installation is required.

## Usage
- <u>Add:</u> Use the add button to add a mod archive. Currently, only `zip` and `rar` formats are supported. The manager will extract the archive and add a manifest file to it if one is not already present. (Which will be always because no one knows about this manager program yet) Subdirectories in the archive will be treated as options/variants of a mod.
- <u>Save:</u> Clicking this will save the current state of the mods in the list. By that I mean if they are enabled or not and what options are used.
- <u>Purge:</u> This will remove all the patch files deployed by the manager. **NOTE:** It will not remove any manually installed patch files. The manager keeps a record of what it has added to the data directory and will only remove those files.
- <u>Deploy:</u> This will purge and then deploy all the patch files of enabled mods renaming the patch file index along the way. **NOTE:** It *will* fail if there are already patch files with the same names as the to-be-deployed files present in the data directory.

## Features to still be added so far
1. [ ] Reordering the mods in the list changing the load/patch order.

## Bugs:
- None are known so far. (But I'm sure that will change)