using HD2ModManagerLib;

var manager = new HD2ModManager(@"D:\SteamLibrary\steamapps\common\Helldivers 2");
manager.AddMod(@"C:\Users\FloCo\Downloads\Helmet Skull Admiral-102-1-0-1719064758.rar");
manager.InstallMods();