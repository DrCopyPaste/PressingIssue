using Services.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace Services.Win32
{
    public class GlobalHotkeyService : IGlobalHotkeyService, IDisposable
    {
        private readonly NLog.Logger logger = null;

        public event EventHandler<GlobalHotkeyServiceEventArgs> KeyEvent;

        public HashSet<string> ModifierKeys { get; private set; }

        // key = hotkeysettingstring, value = currently held down
        private Dictionary<string, bool> hotkeyPressedStates = null;

        private Dictionary<string, Action> quickCastHotkeys = null;
        private Dictionary<string, Action> onReleaseHotkeys = null;

        // repeatable hotkeys? maybe in the future (additional parameter: repeat interval)

        private GlobalKeyboardHook keyboardHook = null;
        private List<string> pressedKeys = null;
        private HashSet<string> pressedNonModifierKeys = null;

        public bool Running { get; private set; } = false;
        public bool ProcessingHotkeys { get; set; } = true;

        public GlobalHotkeyService()
        {
            logger = NLog.LogManager.GetCurrentClassLogger();

            ModifierKeys = new HashSet<string>()
            {
                Keys.Control.ToString(),
                Keys.LControlKey.ToString(),
                Keys.RControlKey.ToString(),
                Keys.LWin.ToString(),
                Keys.RWin.ToString(),
                Keys.Alt.ToString(),
                Keys.LMenu.ToString(),
                Keys.RMenu.ToString(),
                Keys.RShiftKey.ToString(),
                Keys.LShiftKey.ToString(),
            };

            keyboardHook = new GlobalKeyboardHook();

            hotkeyPressedStates = new Dictionary<string, bool>();
            quickCastHotkeys = new Dictionary<string, Action>();
            onReleaseHotkeys = new Dictionary<string, Action>();
            pressedKeys = new List<string>();
            pressedNonModifierKeys = new HashSet<string>();

            Start();
        }

        private string GetPressedKeysAsSetting(List<string> pressedKeys)
        {
            return string.Join('-', pressedKeys.OrderBy(k => k).ToList());
        }

        public void Dispose()
        {
            Stop();
        }

        private void ProcessKeys_Event(object sender, GlobalKeyboardHook.GlobalKeyboardHookEventArgs e)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if (e.KeyDown)
            {
                UpdateNewlyPressedKeys(e);
                var pressedKeysAsConfig = GetPressedKeysAsSetting(pressedKeys);

                if (ProcessingHotkeys)
                {
                    ProcessHotkeysKeyDown(pressedKeysAsConfig);
                }

                HandleCustomEvent(e, pressedKeysAsConfig);
                logger.Info($"processing pressed keys and invoking custom event took: {stopwatch.ElapsedMilliseconds} ms");

                logger.Info($"logging key down - lparam: {e.Key} - key: {e.KeyName} - all keys down: {string.Join('-', pressedKeys)} - without modifiers: {string.Join('-', pressedNonModifierKeys)}");
                logger.Info($"setting string: {pressedKeysAsConfig}");
                logger.Info($"processing KeyDown event took: {stopwatch.ElapsedMilliseconds} ms");
            }
            else if (e.KeyUp)
            {
                UpdateLiftedKeys(e);
                var pressedKeysAsConfig = GetPressedKeysAsSetting(pressedKeys);

                if (ProcessingHotkeys)
                {
                    ProcessHotkeysKeyUp();
                }

                ResetHotkeyPressedStates();

                HandleCustomEvent(e, pressedKeysAsConfig);
                logger.Info($"processing lifted keys and invoking custom event took: {stopwatch.ElapsedMilliseconds} ms");
                logger.Info($"logging key up - lparam: {e.Key} - key: {e.KeyName} - all keys down: {string.Join('-', pressedKeys)} - without modifiers: {string.Join('-', pressedNonModifierKeys)}");
                logger.Info($"processing KeyUp event took: {stopwatch.ElapsedMilliseconds} ms");
            }
        }

        private void ResetHotkeyPressedStates()
        {
            foreach (var keyName in hotkeyPressedStates.Keys.ToList())
            {
                hotkeyPressedStates[keyName] = false;
            }
        }

        private void HandleCustomEvent(GlobalKeyboardHook.GlobalKeyboardHookEventArgs e, string pressedKeysAsConfig)
        {
            try
            {
                KeyEvent?.Invoke(this, new GlobalHotkeyServiceEventArgs(e.KeyDown, pressedKeys, pressedNonModifierKeys, pressedKeysAsConfig));
            }
            catch (Exception ex)
            {
                // "silently" ignore any errors when triggering events
                logger.Error(ex, "An error occurred trying to trigger the custom hotkeyservice event.");
            }
        }

        private void ProcessHotkeysKeyUp()
        {
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
        }

        private void ProcessHotkeysKeyDown(string pressedKeysAsConfig)
        {
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
            else
            {
                ResetHotkeyPressedStates();
            }
        }

        private void UpdateNewlyPressedKeys(GlobalKeyboardHook.GlobalKeyboardHookEventArgs e)
        {
            if (!pressedKeys.Contains(e.KeyName))
            {
                pressedKeys.Add(e.KeyName);
            }

            if (!pressedNonModifierKeys.Contains(e.KeyName) && !ModifierKeys.Contains(e.KeyName))
            {
                pressedNonModifierKeys.Add(e.KeyName);
            }
        }

        private void UpdateLiftedKeys(GlobalKeyboardHook.GlobalKeyboardHookEventArgs e)
        {
            if (pressedKeys.Contains(e.KeyName))
            {
                pressedKeys.Remove(e.KeyName);
            }

            if (pressedNonModifierKeys.Contains(e.KeyName))
            {
                pressedNonModifierKeys.Remove(e.KeyName);
            }
        }

        public void Start(bool processHotkeys = true)
        {
            ProcessingHotkeys = processHotkeys;
            keyboardHook.KeyEvent += ProcessKeys_Event;
            keyboardHook.Start();
            Running = true;
        }

        public void Stop()
        {
            keyboardHook.Stop();
            keyboardHook.KeyEvent -= ProcessKeys_Event;
            Running = false;
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
