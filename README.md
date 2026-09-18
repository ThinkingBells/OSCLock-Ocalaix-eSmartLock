# OSCLock-Ocalaix-eSmartLock

This is a fork of [ZenithVal/OSCLock](https://github.com/ZenithVal/OSCLock), a great little tool for
opening a Bluetooth LE ESmartLock-compatible lock from a Windows PC. All credit for the original app,
protocol reverse-engineering, and OSC/VRChat integration goes to **ZenithVal** and prior contributors —
this fork only adds a desktop GUI and a couple of Windows Bluetooth compatibility fixes on top of their
work, and stays under the same GPLv3 license.

## What this fork adds
- **OSCLockGui.exe** — a small WPF app with a single "open" button, no console window.
- Minimizes to the system tray instead of closing, with a right-click menu (open / show / exit).
- A configurable **global keyboard shortcut** (e.g. Ctrl+M) that opens the lock from anywhere in
  Windows, even while the app is minimized.
- A settings window (⚙) to edit the eSmartLock cloud credentials, toggle "start with Windows" /
  "start minimized", pick the hotkey, and switch the interface language (Català / English) — all
  without hand-editing `config.toml`.
- Config/log now live in `%LOCALAPPDATA%\Ocalaix`, so the app works even when installed somewhere
  read-only for a normal user (e.g. `C:\Program Files\...`).
- Two Bluetooth reliability fixes needed on some Windows 11 setups: explicit
  `RequestAccessAsync`/`OpenAsync` + uncached GATT reads (fixes an `AccessDenied` error on
  `GetCharacteristicsAsync`), and running the BLE scan off the WPF UI thread (the `DeviceWatcher`
  callbacks were unreliable when awaited from an STA thread with a captured `SynchronizationContext`).

---

## How to use OSCLockGui

1. Turn on Bluetooth and stay near the lock (BLE has short range).
2. Run `OSCLockGui.exe`. Press **Open lock** — the padlock icon shows progress
   (searching → opening → opened).
3. Press the ⚙ gear icon to open **Settings**:
   - Your eSmartLock cloud account (username/password) — only needed if your lock is bound
     to the cloud; the app fetches the real device passcode with it.
   - **Start with Windows** / **Start minimized** — launch automatically at login, straight to
     the tray.
   - **Shortcut to open the lock** — click the field and press any combination (default Ctrl+M).
     It works even while the window is hidden in the tray.
   - **Language** — Català / English.
4. Closing the window (✕) or minimizing sends it to the system tray instead of exiting; right-click
   the tray icon for **Open lock / Show window / Exit**.

### Safety
- Bluetooth can be unreliable and drop the connection randomly — **don't put this lock on
  anything that could put you in danger, and always have a backup plan.**
- Theoretically works with any Bluetooth lock using the eSmartLock app (look for the white/green
  branding). Well tested with [this lock](https://amzn.to/3JAGxmm); also reported to work with
  [EseeSmart](https://amzn.to/3PuaTuo), [ELinkSmart](https://amzn.to/3ra1NsM),
  [Pothunder](https://amzn.to/3r1EJfv), and [Dhiedas](https://amzn.to/46t4xBC).

---

## The original console app

The console app (`OSCLock.exe`) that this was forked from — with its OSC/VRChat timer modes,
avatar-parameter integration, and `config.toml` settings — is unchanged and still buildable from
this same source tree (`OSCLock.csproj`). See [ZenithVal/OSCLock](https://github.com/ZenithVal/OSCLock)
for its documentation.

<br>

# Credits & Licenses

- **Original OSCLock app, protocol work, and OSC/VRChat integration** by
  [ZenithVal](https://github.com/ZenithVal) — [github.com/ZenithVal/OSCLock](https://github.com/ZenithVal/OSCLock).
  This fork (the GUI, tray icon, hotkey, settings window, and Windows 11 Bluetooth fixes) is built
  entirely on top of their work and released under the same GPLv3 license.
- Prior programming before git history by @NeetCode 08/2022
- SharpOSC | [MIT Liscense](https://github.com/tecartlab/SharpOSC/blob/master/License.txt)
- OSCQuery | [MIT Liscense](https://github.com/vrchat-community/vrc-oscquery-lib/blob/main/License.md)
- Tomlet | [MIT Liscense](https://github.com/SamboyCoding/Tomlet/blob/master/LICENSE)
- FluentColorConsole | [MIT Liscense](https://github.com/developer82/FluentColorConsole/blob/master/LICENSE)
- App Heart Icon | [Game-icons.net](https://game-icons.net/1x1/delapouite/locked-heart.html) under [CC by 3.0](https://creativecommons.org/licenses/by/3.0/)
