using System;
using System.IO;
using OSCLock.Gui;
using Tomlet;

namespace OSCLock.Configs
{
	// Minimal config manager for the GUI: reads/writes config.toml from a
	// writable per-user folder, without the console app's OSC/Timer/encryption
	// features.
	public static class ConfigManager
	{
		public static MainConfig ApplicationConfig;

		static ConfigManager()
		{
			EnsureConfigFileExists();
			InitConfig();
		}

		private static void EnsureConfigFileExists()
		{
			if (File.Exists(AppPaths.ConfigFilePath)) return;

			// Migrate a config.toml sitting next to the exe (e.g. from before the
			// app was moved into a read-only folder such as Program Files) into
			// the writable per-user data folder, so existing credentials survive.
			var legacyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.toml");
			if (File.Exists(legacyPath))
			{
				File.Copy(legacyPath, AppPaths.ConfigFilePath);
			}
		}

		private static void InitConfig()
		{
			var doc = TomlParser.ParseFile(AppPaths.ConfigFilePath);
			ApplicationConfig = TomletMain.To<MainConfig>(doc);
		}

		public static void Save()
		{
			var configData = TomletMain.DocumentFrom(ApplicationConfig);
			File.WriteAllText(AppPaths.ConfigFilePath, configData.SerializedValue);
			InitConfig();
		}
	}
}
