using Services.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Services.Win32
{
    public class GlobalHotkeyService : IGlobalHotkeyService, IDisposable
    {
        private readonly NLog.Logger logger = null;

        // key = hotkeysettingstring, value = currently held down
        private Dictionary<string, bool> hotkeyPressedStates = null;

        private Dictionary<string, Action> quickCastHotkeys = null;
        private Dictionary<string, Action> onReleaseHotkeys = null;

        // repeatable hotkeys? maybe in the future (additional parameter: repeat interval)

        private GlobalKeyboardHook keyboardHook = null;
        private List<string> pressedKeys = null;
        private HashSet<string> pressedNonModifierKeys = null;

        public GlobalHotkeyService()
        {
            logger = NLog.LogManager.GetCurrentClassLogger();

            keyboardHook = new GlobalKeyboardHook();

            hotkeyPressedStates = new Dictionary<string, bool>();
            quickCastHotkeys = new Dictionary<string, Action>();
            onReleaseHotkeys = new Dictionary<string, Action>();
            pressedKeys = new List<string>();
            pressedNonModifierKeys = new HashSet<string>();

            StartHook();
        }

        public string GetPressedKeysAsSetting()
        {
            return string.Join('-', pressedKeys.OrderBy(k => k).ToList());
        }

        public void Dispose()
        {
            StopHook();
        }

        private void Mahook_KeyEvent(object sender, GlobalKeyboardHook.GlobalKeyboardHookEventArgs e)
        {
            if (e.keyDown)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                UpdateNewlyPressedKeys(e);

                var pressedKeysAsConfig = GetPressedKeysAsSetting();

                if (hotkeyPressedStates.ContainsKey(pressedKeysAsConfig))
                {
                    var couldTriggerQuickCast = !hotkeyPressedStates[pressedKeysAsConfig];
                    hotkeyPressedStates[pressedKeysAsConfig] = true;

                    if (couldTriggerQuickCast && quickCastHotkeys.Any() && quickCastHotkeys.ContainsKey(pressedKeysAsConfig))
                    {
                        try
                        {
                            quickCastHotkeys[pressedKeysAsConfig].Invoke();
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, $"An error occurred trying to trigger action for quick cast hotkey '{pressedKeysAsConfig}'");
                        }
                    }
                }

                logger.Info($"logging key down - lparam: {e.lParam} - key: {e.keyName} - all keys down: {string.Join('-', pressedKeys)} - without modifiers: {string.Join('-', pressedNonModifierKeys)}");
                logger.Info($"setting string: {GetPressedKeysAsSetting()}");

                stopwatch.Stop();
                logger.Info($"processing KeyDown event took: {stopwatch.ElapsedMilliseconds} ms");
            }
            else if (e.keyUp)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                UpdateLiftedKeys(e);

                if (onReleaseHotkeys.Any())
                {
                    // only one action per hotkey allowed atm (dictionary), but that may change
                    var hotkeysWaitingForRelease = onReleaseHotkeys.Where(h => hotkeyPressedStates.ContainsKey(h.Key) && hotkeyPressedStates[h.Key]);

                    if (hotkeysWaitingForRelease.Any())
                    {
                        foreach (var hotkeyAction in hotkeysWaitingForRelease)
                        {
                            try
                            {
                                hotkeyAction.Value.Invoke();
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex, $"An error occurred trying to trigger action for on-release hotkey '{hotkeyAction.Key}'");
                            }
                        }
                    }
                }

                foreach (var keyName in hotkeyPressedStates.Keys.ToList())
                {
                    hotkeyPressedStates[keyName] = false;
                }

                logger.Info($"logging key up - lparam: {e.lParam} - key: {e.keyName} - all keys down: {string.Join('-', pressedKeys)} - without modifiers: {string.Join('-', pressedNonModifierKeys)}");

                stopwatch.Stop();
                logger.Info($"processing KeyUp event took: {stopwatch.ElapsedMilliseconds} ms");
            }
        }

        private void UpdateNewlyPressedKeys(GlobalKeyboardHook.GlobalKeyboardHookEventArgs e)
        {
            if (!pressedKeys.Contains(e.keyName))
            {
                pressedKeys.Add(e.keyName);
            }

            if (!pressedNonModifierKeys.Contains(e.keyName) && !keyboardHook.ModifierKeys.Contains(e.keyName))
            {
                pressedNonModifierKeys.Add(e.keyName);
            }
        }

        private void UpdateLiftedKeys(GlobalKeyboardHook.GlobalKeyboardHookEventArgs e)
        {
            if (pressedKeys.Contains(e.keyName))
            {
                pressedKeys.Remove(e.keyName);
            }

            if (pressedNonModifierKeys.Contains(e.keyName))
            {
                pressedNonModifierKeys.Remove(e.keyName);
            }
        }

        public string GetCurrentKeysPressed()
        {
            throw new NotImplementedException();
        }

        public void StartHook()
        {
            keyboardHook.KeyEvent += Mahook_KeyEvent;
            keyboardHook.Start();
        }

        public void StopHook()
        {
            keyboardHook.Stop();
        }

        public void AddOrUpdateQuickCastHotkey(string settingString, Action hotkeyAction)
        {
            AddOrUpdateHotkeyState(settingString);

            if (this.quickCastHotkeys.ContainsKey(settingString))
            {
                this.quickCastHotkeys[settingString] = hotkeyAction;
            }
            else
            {
                this.quickCastHotkeys.Add(settingString, hotkeyAction);
            }
        }

        public void AddOrUpdateOnReleaseHotkey(string settingString, Action hotkeyAction)
        {
            AddOrUpdateHotkeyState(settingString);

            if (this.onReleaseHotkeys.ContainsKey(settingString))
            {
                this.onReleaseHotkeys[settingString] = hotkeyAction;
            }
            else
            {
                this.onReleaseHotkeys.Add(settingString, hotkeyAction);
            }
        }

        private void AddOrUpdateHotkeyState(string settingString)
        {
            if (this.hotkeyPressedStates.ContainsKey(settingString))
            {
                this.hotkeyPressedStates[settingString] = false;
            }
            else
            {
                this.hotkeyPressedStates.Add(settingString, false);
            }
        }
    }


}
