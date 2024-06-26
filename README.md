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
- [ ] Reordering the mods in the list changing the load/patch order.
- [ ] A dialog to create a mod. Intended to do the manifest writing for you.

## Bugs:
- All are fixed for now.

## For mod developers:
If you are a mod developer and want your mod to work best with this mod manager this is the section for you!
When adding a mod to the manager it will check if your archive file contains a manifest.json file. If it finds one it will use it to display and deploy your mod. If it can't find one it will create one upon extraction and treat all level-one subdirectories as variants of the mod.
What follows now is an example manifest.json file to give you the right idea. I'll be using [CursedLibertine's](https://next.nexusmods.com/profile/CursedLibertine/about-me) [Jane Diver](https://www.nexusmods.com/helldivers2/mods/69) mod as an example.
```json
﻿{
    "Guid": "f9125200-1cc8-484d-acc9-1bfd8fdce4fb", //A GUID this is nothing visible but must be uniqe. The manager uses it to identify your mod.
    "Name": "Jane Helldiver 4K v1.04", //This is the display name of your mod.
    "Description": "Locally imported mod.", //This is a shord description of your mod.
    "IconPath": null, //This is a optional path to a image file if you want your mod to have a icon in the manager.
    "Options": [ //These are the variants of the mod. The listed names must be the exact same as the sub directories in your archive.
        "Jane Helldiver 4K - Skin B",
        "Jane Helldiver 4K - Skin A"
    ]
}
```
I hope this helps whoever is trying to understand my tool.
If there are any more questions please message me and I will try my best to assist you.

---
## Change log

### 0.1.0.0 (Beta)
- Initial release

### 0.1.1.0 (Beta)
- Fixed a bug where the indexing of patch files got messed up if patch files had different names.
- Added a work indicator when adding/purging/deploying as to not block the UI thread.