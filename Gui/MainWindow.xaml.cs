using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using OSCLock.Bluetooth;
using Forms = System.Windows.Forms;

namespace OSCLock.Gui
{
	public partial class MainWindow : Window
	{
		private static readonly Brush ClosedColor = (Brush)new BrushConverter().ConvertFromString("#3A3F52");
		private static readonly Brush ClosedShackleColor = (Brush)new BrushConverter().ConvertFromString("#5B6072");
		private static readonly Brush OpenColor = (Brush)new BrushConverter().ConvertFromString("#1F6D4A");
		private static readonly Brush OpenShackleColor = (Brush)new BrushConverter().ConvertFromString("#3FBE85");
		private static readonly Brush BusyColor = (Brush)new BrushConverter().ConvertFromString("#5A4A20");
		private static readonly Brush BusyShackleColor = (Brush)new BrushConverter().ConvertFromString("#D8A23B");

		private const int HOTKEY_ID = 0x9000;
		private const int WM_HOTKEY = 0x0312;

		[DllImport("user32.dll")]
		private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

		[DllImport("user32.dll")]
		private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

		private HwndSource hwndSource;
		private Forms.NotifyIcon trayIcon;
		private Forms.ToolStripMenuItem trayOpenItem;
		private Forms.ToolStripMenuItem trayShowItem;
		private Forms.ToolStripMenuItem trayExitItem;
		private bool isExiting = false;
		private bool isBusy = false;

		public MainWindow()
		{
			InitializeComponent();
			SetupTrayIcon();
			ApplyLanguage();
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);
			hwndSource = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
			hwndSource.AddHook(WndProc);
			RegisterConfiguredHotkey();
		}

		public void RegisterConfiguredHotkey()
		{
			if (hwndSource == null) return;

			UnregisterHotKey(hwndSource.Handle, HOTKEY_ID);
			var modifiers = (uint)StartupSettings.HotkeyModifiers;
			var vk = (uint)StartupSettings.HotkeyVirtualKey;
			if (!RegisterHotKey(hwndSource.Handle, HOTKEY_ID, modifiers, vk))
			{
				Console.WriteLine("[GUI] Failed to register global hotkey (potser ja usada per un altre programa)");
			}

			UpdateFooterText();
		}

		public void ApplyLanguage()
		{
			OpenButton.Content = Strings.OpenButton;
			if (!isBusy) SetClosedVisual(Strings.Ready);
			UpdateFooterText();

			if (trayOpenItem != null) trayOpenItem.Text = Strings.TrayOpenLock;
			if (trayShowItem != null) trayShowItem.Text = Strings.TrayShowWindow;
			if (trayExitItem != null) trayExitItem.Text = Strings.TrayExit;
		}

		private void UpdateFooterText()
		{
			var hotkeyText = Strings.FormatHotkey(StartupSettings.HotkeyModifiers, StartupSettings.HotkeyVirtualKey);
			FooterText.Text = Strings.Footer(hotkeyText);
		}

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
			{
				handled = true;
				_ = TriggerOpenAsync();
			}
			return IntPtr.Zero;
		}

		private void SetupTrayIcon()
		{
			trayIcon = new Forms.NotifyIcon();
			try
			{
				trayIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
			}
			catch
			{
				trayIcon.Icon = System.Drawing.SystemIcons.Application;
			}
			trayIcon.Text = "OSCLock";
			trayIcon.Visible = true;
			trayIcon.DoubleClick += (s, e) => ShowWindowFromTray();

			var menu = new Forms.ContextMenuStrip();
			trayOpenItem = new Forms.ToolStripMenuItem(Strings.TrayOpenLock, null, (s, e) => { _ = TriggerOpenAsync(); });
			trayShowItem = new Forms.ToolStripMenuItem(Strings.TrayShowWindow, null, (s, e) => ShowWindowFromTray());
			trayExitItem = new Forms.ToolStripMenuItem(Strings.TrayExit, null, (s, e) =>
			{
				isExiting = true;
				Close();
			});
			menu.Items.Add(trayOpenItem);
			menu.Items.Add(trayShowItem);
			menu.Items.Add(new Forms.ToolStripSeparator());
			menu.Items.Add(trayExitItem);
			trayIcon.ContextMenuStrip = menu;
		}

		private void ShowWindowFromTray()
		{
			Show();
			WindowState = WindowState.Normal;
			Activate();
		}

		private void Window_StateChanged(object sender, EventArgs e)
		{
			if (WindowState == WindowState.Minimized)
			{
				Hide();
			}
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			if (!isExiting)
			{
				e.Cancel = true;
				Hide();
				return;
			}

			if (hwndSource != null)
			{
				UnregisterHotKey(hwndSource.Handle, HOTKEY_ID);
			}
			trayIcon?.Dispose();
			Application.Current.Shutdown();
		}

		private void OpenButton_Click(object sender, RoutedEventArgs e)
		{
			_ = TriggerOpenAsync();
		}

		private void SettingsButton_Click(object sender, RoutedEventArgs e)
		{
			var settings = new SettingsWindow { Owner = this };
			settings.ShowDialog();
			RegisterConfiguredHotkey();
			ApplyLanguage();
		}

		private async Task TriggerOpenAsync()
		{
			if (isBusy) return;
			isBusy = true;
			OpenButton.IsEnabled = false;
			SetBusyVisual(Strings.Searching);

			try
			{
				Console.WriteLine("[GUI] Triggered, starting scan...");

				// The BLE DeviceWatcher/GATT callbacks are unreliable when awaited
				// from the WPF UI thread (STA + captured SynchronizationContext),
				// so run the whole scan on a plain MTA threadpool thread instead,
				// same as the console app does by default.
				var lockDevice = await Task.Run(async () =>
				{
					var scanTask = BleScanner.FindESmartLock();
					var timeoutTask = Task.Delay(TimeSpan.FromSeconds(25));
					var finished = await Task.WhenAny(scanTask, timeoutTask);

					if (finished == timeoutTask)
					{
						Console.WriteLine("[GUI] Scan timed out after 25s");
						BleScanner.CancelScan();
						return null;
					}

					return await scanTask;
				});

				if (lockDevice == null)
				{
					SetClosedVisual(Strings.NotFound);
					return;
				}

				Console.WriteLine("[GUI] Lock found, unlocking...");
				SetBusyVisual(Strings.Opening);
				await Task.Run(() => lockDevice.Unlock());

				Console.WriteLine("[GUI] Unlock call finished");
				SetOpenVisual(Strings.Opened);
				await Task.Delay(TimeSpan.FromSeconds(4));
				SetClosedVisual(Strings.Ready);
			}
			catch (Exception ex)
			{
				Console.WriteLine("[GUI] Exception: " + ex);
				SetClosedVisual(Strings.ErrorPrefix + ex.Message);
			}
			finally
			{
				OpenButton.IsEnabled = true;
				isBusy = false;
			}
		}

		private void SetBusyVisual(string status)
		{
			StatusText.Text = status;
			Body.Fill = BusyColor;
			Shackle.Stroke = BusyShackleColor;
			ShackleRotate.Angle = 0;
			SetTrayText(status);
		}

		private void SetOpenVisual(string status)
		{
			StatusText.Text = status;
			Body.Fill = OpenColor;
			Shackle.Stroke = OpenShackleColor;
			ShackleRotate.Angle = -38;
			SetTrayText(status);
			trayIcon?.ShowBalloonTip(3000, "OSCLock", Strings.TrayLockOpened, Forms.ToolTipIcon.Info);
		}

		private void SetClosedVisual(string status)
		{
			StatusText.Text = status;
			Body.Fill = ClosedColor;
			Shackle.Stroke = ClosedShackleColor;
			ShackleRotate.Angle = 0;
			SetTrayText(status);
		}

		private void SetTrayText(string status)
		{
			if (trayIcon == null) return;
			var text = "OSCLock - " + status;
			if (text.Length > 63) text = text.Substring(0, 60) + "...";
			trayIcon.Text = text;
		}
	}
}
