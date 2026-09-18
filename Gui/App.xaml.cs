using System;
using System.IO;
using System.Windows;
using System.Windows.Interop;

namespace OSCLock.Gui
{
	public partial class App : Application
	{
		public App()
		{
			try
			{
				var writer = new StreamWriter(AppPaths.LogFilePath, false) { AutoFlush = true };
				Console.SetOut(writer);
				Console.SetError(writer);
				Console.WriteLine("=== OSCLockGui started " + DateTime.Now + " ===");
			}
			catch
			{
				// Logging is best-effort only.
			}
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			var window = new MainWindow();
			MainWindow = window;

			// Force the Win32 window handle to exist (needed for the global
			// hotkey and tray icon) without necessarily showing the window.
			new WindowInteropHelper(window).EnsureHandle();

			if (!StartupSettings.StartMinimized)
			{
				window.Show();
			}
		}
	}
}
