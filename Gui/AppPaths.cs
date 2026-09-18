using System;
using System.IO;

namespace OSCLock.Gui
{
	// Writable, per-user location for state the app needs to save (config.toml,
	// debug log). The exe itself may live somewhere read-only for a normal user
	// (e.g. C:\Program Files\...), so nothing gets written next to it.
	public static class AppPaths
	{
		public static string DataDirectory
		{
			get
			{
				var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Ocalaix");
				Directory.CreateDirectory(dir);
				return dir;
			}
		}

		public static string ConfigFilePath => Path.Combine(DataDirectory, "config.toml");
		public static string LogFilePath => Path.Combine(DataDirectory, "gui-debug.log");
	}
}
