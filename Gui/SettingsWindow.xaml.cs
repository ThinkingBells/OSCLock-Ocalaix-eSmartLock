using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using OSCLock.Configs;

namespace OSCLock.Gui
{
	public partial class SettingsWindow : Window
	{
		private static readonly Brush OkColor = (Brush)new BrushConverter().ConvertFromString("#3FBE85");
		private static readonly Brush ErrorColor = (Brush)new BrushConverter().ConvertFromString("#E0725B");

		private ModifierKeys capturedModifiers;
		private Key capturedKey;
		private AppLanguage pendingLanguage;

		public SettingsWindow()
		{
			InitializeComponent();

			var cfg = ConfigManager.ApplicationConfig.ESmartConfig;
			UsernameBox.Text = cfg?.apiUsername ?? "";
			PasswordInput.Password = cfg?.apiPassword ?? "";

			StartWithWindowsBox.IsChecked = StartupSettings.StartWithWindows;
			StartMinimizedBox.IsChecked = StartupSettings.StartMinimized;

			capturedModifiers = FromWin32Modifiers(StartupSettings.HotkeyModifiers);
			capturedKey = KeyInterop.KeyFromVirtualKey(StartupSettings.HotkeyVirtualKey);

			pendingLanguage = Strings.Current;
			LanguageBox.SelectedIndex = pendingLanguage == AppLanguage.English ? 1 : 0;

			ApplyLanguage();
		}

		private void ApplyLanguage()
		{
			Title = Strings.SettingsTitle;
			UsernameLabelText.Text = Strings.UsernameLabel;
			PasswordLabelText.Text = Strings.PasswordLabel;
			StartWithWindowsBox.Content = Strings.StartWithWindowsLabel;
			StartMinimizedBox.Content = Strings.StartMinimizedLabel;
			HotkeyLabelText.Text = Strings.HotkeyLabel;
			HotkeyBox.Text = FormatHotkey(capturedModifiers, capturedKey);
			LanguageLabelText.Text = Strings.LanguageLabel;
			CancelButtonText.Content = Strings.CancelButton;
			SaveButtonText.Content = Strings.SaveButton;
		}

		private void LanguageBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (LanguageBox.SelectedItem is ComboBoxItem item)
			{
				pendingLanguage = (string)item.Tag == "English" ? AppLanguage.English : AppLanguage.Catalan;
			}
		}

		private void HotkeyBox_GotFocus(object sender, RoutedEventArgs e)
		{
			HotkeyBox.Text = Strings.HotkeyPromptRecording;
		}

		private void HotkeyBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			e.Handled = true;
			var key = e.Key == Key.System ? e.SystemKey : e.Key;

			// Ignore a lone modifier press; wait for a real key held together with one.
			if (key == Key.LeftCtrl || key == Key.RightCtrl ||
				key == Key.LeftAlt || key == Key.RightAlt ||
				key == Key.LeftShift || key == Key.RightShift ||
				key == Key.LWin || key == Key.RWin)
			{
				return;
			}

			var modifiers = Keyboard.Modifiers;
			if (modifiers == ModifierKeys.None)
			{
				StatusMessage.Foreground = ErrorColor;
				StatusMessage.Text = Strings.HotkeyNeedsModifier;
				return;
			}

			capturedModifiers = modifiers;
			capturedKey = key;
			HotkeyBox.Text = FormatHotkey(capturedModifiers, capturedKey);
			StatusMessage.Text = "";
		}

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				ConfigManager.ApplicationConfig.ESmartConfig.apiUsername = UsernameBox.Text.Trim();
				ConfigManager.ApplicationConfig.ESmartConfig.apiPassword = PasswordInput.Password;
				ConfigManager.Save();

				StartupSettings.StartWithWindows = StartWithWindowsBox.IsChecked == true;
				StartupSettings.StartMinimized = StartMinimizedBox.IsChecked == true;

				StartupSettings.HotkeyModifiers = (int)ToWin32Modifiers(capturedModifiers);
				StartupSettings.HotkeyVirtualKey = KeyInterop.VirtualKeyFromKey(capturedKey);

				StartupSettings.Language = pendingLanguage;
				Strings.Current = pendingLanguage;
				ApplyLanguage();

				StatusMessage.Foreground = OkColor;
				StatusMessage.Text = Strings.SavedOk;
			}
			catch (Exception ex)
			{
				StatusMessage.Foreground = ErrorColor;
				StatusMessage.Text = Strings.SaveErrorPrefix + ex.Message;
			}
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private static string FormatHotkey(ModifierKeys modifiers, Key key) => Strings.FormatHotkey(modifiers, key);

		private static uint ToWin32Modifiers(ModifierKeys modifiers)
		{
			uint result = 0;
			if (modifiers.HasFlag(ModifierKeys.Alt)) result |= 0x0001;
			if (modifiers.HasFlag(ModifierKeys.Control)) result |= 0x0002;
			if (modifiers.HasFlag(ModifierKeys.Shift)) result |= 0x0004;
			if (modifiers.HasFlag(ModifierKeys.Windows)) result |= 0x0008;
			return result;
		}

		private static ModifierKeys FromWin32Modifiers(int modifiers)
		{
			var result = ModifierKeys.None;
			if ((modifiers & 0x0001) != 0) result |= ModifierKeys.Alt;
			if ((modifiers & 0x0002) != 0) result |= ModifierKeys.Control;
			if ((modifiers & 0x0004) != 0) result |= ModifierKeys.Shift;
			if ((modifiers & 0x0008) != 0) result |= ModifierKeys.Windows;
			return result;
		}
	}
}
