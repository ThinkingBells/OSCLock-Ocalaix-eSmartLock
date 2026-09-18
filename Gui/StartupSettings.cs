using Microsoft.Win32;

namespace OSCLock.Gui
{
	// Small helper for the two GUI-only preferences (start with Windows / start
	// minimized). Kept out of config.toml since those are console+GUI shared
	// lock/cloud settings, not GUI window behaviour.
	public static class StartupSettings
	{
		private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
		private const string AppKeyPath = @"Software\OSCLock\Gui";
		private const string RunValueName = "OSCLockGui";

		public static bool StartWithWindows
		{
			get
			{
				using (var key = Registry.CurrentUser.OpenSubKey(RunKeyPath))
				{
					return key?.GetValue(RunValueName) != null;
				}
			}
			set
			{
				using (var key = Registry.CurrentUser.CreateSubKey(RunKeyPath))
				{
					if (value)
					{
						var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
						key.SetValue(RunValueName, "\"" + exePath + "\"");
					}
					else
					{
						key.DeleteValue(RunValueName, throwOnMissingValue: false);
					}
				}
			}
		}

		public static bool StartMinimized
		{
			get
			{
				using (var key = Registry.CurrentUser.OpenSubKey(AppKeyPath))
				{
					var value = key?.GetValue("StartMinimized");
					return value is int i && i != 0;
				}
			}
			set
			{
				using (var key = Registry.CurrentUser.CreateSubKey(AppKeyPath))
				{
					key.SetValue("StartMinimized", value ? 1 : 0, RegistryValueKind.DWord);
				}
			}
		}

		// Win32 MOD_CONTROL (0x0002) + virtual-key 'M' (0x4D), i.e. Ctrl+M.
		private const int DefaultHotkeyModifiers = 0x0002;
		private const int DefaultHotkeyVirtualKey = 0x4D;

		public static int HotkeyModifiers
		{
			get
			{
				using (var key = Registry.CurrentUser.OpenSubKey(AppKeyPath))
				{
					var value = key?.GetValue("HotkeyModifiers");
					return value is int i ? i : DefaultHotkeyModifiers;
				}
			}
			set
			{
				using (var key = Registry.CurrentUser.CreateSubKey(AppKeyPath))
				{
					key.SetValue("HotkeyModifiers", value, RegistryValueKind.DWord);
				}
			}
		}

		public static int HotkeyVirtualKey
		{
			get
			{
				using (var key = Registry.CurrentUser.OpenSubKey(AppKeyPath))
				{
					var value = key?.GetValue("HotkeyVirtualKey");
					return value is int i ? i : DefaultHotkeyVirtualKey;
				}
			}
			set
			{
				using (var key = Registry.CurrentUser.CreateSubKey(AppKeyPath))
				{
					key.SetValue("HotkeyVirtualKey", value, RegistryValueKind.DWord);
				}
			}
		}

		public static AppLanguage Language
		{
			get
			{
				using (var key = Registry.CurrentUser.OpenSubKey(AppKeyPath))
				{
					var value = key?.GetValue("Language");
					return value is int i && i == 1 ? AppLanguage.English : AppLanguage.Catalan;
				}
			}
			set
			{
				using (var key = Registry.CurrentUser.CreateSubKey(AppKeyPath))
				{
					key.SetValue("Language", value == AppLanguage.English ? 1 : 0, RegistryValueKind.DWord);
				}
			}
		}
	}
}
