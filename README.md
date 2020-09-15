# PressingIssue

PressingIssue provides wrappers to low level WinApi32 calls to hook into windows keyboard events.

# How to Use

For details on how to implement/use the provided classes refer to the test projects inside the solution.

# GlobalKeyboardHook

This class enables you to hook into low level windows key events using SetWindowsHookEx.

The following HotkeyServices already use this class, so if you are just after using hotkeys and do not need the low level key event itself you wont need to create instances of GlobalKeyboardHook yourself.

Typically you only need to create an instance, start it and subscribe to the hook's KeyEvent:

```cs
var keyboardHook = new GlobalKeyboardHook();
keyboardHook.KeyEvent += KeyboardHookEvent; // declare this event
keyboardHook.Start();
```

# HotkeyServices

By default, both hotkey services start directly in hotkey capturing mode when an instance is created.

But they differ in how you pass a hotkey setting to them.
To get the setting string of a hotkey (for instance to update your hotkey configurations) use hotkey service's respective KeyEvent.AsSettingString

# MultiKeyGlobalHotkeyService

MultiKeyGlobalHotkeyService enables you to add as many non-modifier keys as you like to your hotkeys (limited by your keyboard hardware of course).

This is achieved by some internal "accounting" of pressed keys using dictionaries, which of course has "some" performance penalties. (see logging for details)

```cs
var hotkeyService = new MultiKeyGlobalHotkeyService();
hotkeyService.KeyEvent += hotkeyServiveKeyEvent; // (optional) declare this event if you want to react to it

hotkeyService.ProcessingHotkeys = false; // toggle reacting to hotkeys if needed

// add hotkeys that should trigger when released like this
hotkeyService.AddOrUpdateOnReleaseHotkey(
  "Key=Pause; Win=False; Alt=False; Ctrl=False; Shift=False",
  () => System.Console.WriteLine("you triggered [Pause] on release"));

// add hotkeys that should trigger on key down like this
hotkeyService.AddOrUpdateQuickCastHotkey(
  "Key=F12; Win=False; Alt=False; Ctrl=False; Shift=False",
  () => System.Console.WriteLine("you triggered [F12] with quickcast (on key down)"));
```

# SimpleGlobalHotkeyService

SimpleGlobalHotkeyService uses GetAsyncKeyState internally to monitor pressed modifier keys and therefore only allows for one additional non-modifier key to be used.

```cs
var hotkeyService = new SimpleGlobalHotkeyService();
hotkeyService.KeyEvent += hotkeyServiveKeyEvent; // (optional) declare this event if you want to react to it

hotkeyService.ProcessingHotkeys = false; // toggle reacting to hotkeys if needed

// add hotkeys that should trigger when released like this
hotkeyService.AddOrUpdateOnReleaseHotkey(
  "Pause",
  () => System.Console.WriteLine("you triggered [Pause] on release"));

// add hotkeys that should trigger on key down like this
hotkeyService.AddOrUpdateQuickCastHotkey(
  "F12",
  () => System.Console.WriteLine("you triggered [F12] with quickcast (on key down)"));
```
