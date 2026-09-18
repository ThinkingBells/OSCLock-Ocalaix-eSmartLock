using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Interop;

namespace OSCLock.Gui
{
	public enum AppLanguage
	{
		Catalan,
		English
	}

	// Small hand-rolled localization table (Catalan / English) instead of full
	// satellite-assembly resources, since the app only has a handful of strings.
	public static class Strings
	{
		public static AppLanguage Current { get; set; } = StartupSettings.Language;

		private static string Pick(string ca, string en) => Current == AppLanguage.Catalan ? ca : en;

		// Main window
		public static string Ready => Pick("Preparat", "Ready");
		public static string Searching => Pick("Cercant el pany...", "Searching for the lock...");
		public static string Opening => Pick("Obrint...", "Opening...");
		public static string Opened => Pick("Obert", "Opened");
		public static string NotFound => Pick("No s'ha trobat el pany. Torna-ho a provar.", "Lock not found. Try again.");
		public static string CouldNotConnect => Pick("No s'ha pogut connectar al pany.", "Could not connect to the lock.");
		public static string ErrorPrefix => Pick("Error: ", "Error: ");
		public static string OpenButton => Pick("OBRIR PANY", "OPEN LOCK");
		public static string TrayLockOpened => Pick("Pany obert", "Lock opened");
		public static string TrayOpenLock => Pick("Obrir pany", "Open lock");
		public static string TrayShowWindow => Pick("Mostra la finestra", "Show window");
		public static string TrayExit => Pick("Surt", "Exit");

		public static string Footer(string hotkeyText) =>
			Pick($"OSCLock  ·  {hotkeyText} obre  ·  minimitza per anar a la safata",
				 $"OSCLock  ·  {hotkeyText} opens  ·  minimize to go to tray");

		// Settings window
		public static string SettingsTitle => Pick("Configuració", "Settings");
		public static string UsernameLabel => Pick("Usuari (compte eSmartLock)", "Username (eSmartLock account)");
		public static string PasswordLabel => Pick("Contrasenya", "Password");
		public static string StartWithWindowsLabel => Pick("Inicia amb el Windows", "Start with Windows");
		public static string StartMinimizedLabel => Pick("Inicia minimitzat (a la safata)", "Start minimized (to tray)");
		public static string HotkeyLabel => Pick("Drecera per obrir el pany", "Shortcut to open the lock");
		public static string HotkeyPlaceholder => Pick("Fes clic i prem la combinació...", "Click and press the combination...");
		public static string HotkeyPromptRecording => Pick("Prem la combinació ara...", "Press the combination now...");
		public static string HotkeyNeedsModifier => Pick("Cal combinar-ho amb Ctrl, Alt, Maj o Win.", "It must be combined with Ctrl, Alt, Shift or Win.");
		public static string SavedOk => Pick("Desat correctament.", "Saved successfully.");
		public static string SaveErrorPrefix => Pick("Error en desar: ", "Error while saving: ");
		public static string CancelButton => Pick("Cancel·la", "Cancel");
		public static string SaveButton => Pick("Desa", "Save");
		public static string LanguageLabel => Pick("Idioma", "Language");

		public static string ModifierCtrl => "Ctrl";
		public static string ModifierAlt => "Alt";
		public static string ModifierShift => Pick("Maj", "Shift");
		public static string ModifierWin => "Win";

		public static string FormatHotkey(ModifierKeys modifiers, Key key)
		{
			var parts = new List<string>();
			if (modifiers.HasFlag(ModifierKeys.Control)) parts.Add(ModifierCtrl);
			if (modifiers.HasFlag(ModifierKeys.Alt)) parts.Add(ModifierAlt);
			if (modifiers.HasFlag(ModifierKeys.Shift)) parts.Add(ModifierShift);
			if (modifiers.HasFlag(ModifierKeys.Windows)) parts.Add(ModifierWin);
			parts.Add(key.ToString());
			return string.Join("+", parts);
		}

		public static string FormatHotkey(int win32Modifiers, int virtualKey)
		{
			var parts = new List<string>();
			if ((win32Modifiers & 0x0002) != 0) parts.Add(ModifierCtrl);
			if ((win32Modifiers & 0x0001) != 0) parts.Add(ModifierAlt);
			if ((win32Modifiers & 0x0004) != 0) parts.Add(ModifierShift);
			if ((win32Modifiers & 0x0008) != 0) parts.Add(ModifierWin);
			parts.Add(KeyInterop.KeyFromVirtualKey(virtualKey).ToString());
			return string.Join("+", parts);
		}
	}
}
