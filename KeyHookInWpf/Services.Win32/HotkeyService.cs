using Services.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Services.Win32
{
    public class HotkeyService : IHotkeyService, IDisposable
    {
        private readonly NLog.Logger logger = null;

        private Dictionary<string, HotkeyAction> quickCastHotkeys = null;
        private Dictionary<string, HotkeyAction> onReleaseHotkeys = null;

        private KeyboardHook keyboardHook = null;
        private List<string> pressedKeys = null;
        private HashSet<string> nonModifierKeys = null;

        public HotkeyService()
        {
            logger = NLog.LogManager.GetCurrentClassLogger();

            keyboardHook = new KeyboardHook();
            quickCastHotkeys = new Dictionary<string, HotkeyAction>();
            onReleaseHotkeys = new Dictionary<string, HotkeyAction>();
            pressedKeys = new List<string>();
            nonModifierKeys = new HashSet<string>();

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

        private void Mahook_KeyEvent(object sender, KeyboardHook.HotkeyServiceHookEventArgs e)
        {
            if (e.keyDown)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                UpdateNewlyPressedKeys(e);

                var pressedKeysAsConfig = GetPressedKeysAsSetting();

                if (quickCastHotkeys.Any() && quickCastHotkeys.ContainsKey(pressedKeysAsConfig))
                {
                    if (!quickCastHotkeys[pressedKeysAsConfig].CurrentlyHeld)
                    {
                        quickCastHotkeys[pressedKeysAsConfig].CurrentlyHeld = true;

                        try
                        {
                            quickCastHotkeys[pressedKeysAsConfig].Action.Invoke();
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, $"An error occurred trying to trigger action for quick cast hotkey '{pressedKeysAsConfig}'");
                        }
                    }
                }

                if (onReleaseHotkeys.Any() && onReleaseHotkeys.ContainsKey(pressedKeysAsConfig))
                {
                    if (!onReleaseHotkeys[pressedKeysAsConfig].CurrentlyHeld)
                    {
                        onReleaseHotkeys[pressedKeysAsConfig].CurrentlyHeld = true;
                    }
                }

                logger.Info($"logging key down - lparam: {e.lParam} - key: {e.keyName} - all keys down: {string.Join('-', pressedKeys)} - without modifiers: {string.Join('-', nonModifierKeys)}");
                logger.Info($"setting string: {GetPressedKeysAsSetting()}");

                stopwatch.Stop();
                logger.Info($"processing KeyDown event took: {stopwatch.ElapsedMilliseconds} ms");
                // testing for key down hotkeys
            }
            else if (e.keyUp)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                UpdateLiftedKeys(e);

                if (onReleaseHotkeys.Any())
                {
                    // only one action per hotkey allowed atm (dictionary), but that may change
                    var hotkeysWaitingForRelease = onReleaseHotkeys.Where(h => h.Value.CurrentlyHeld);

                    if (hotkeysWaitingForRelease.Any())
                    {
                        foreach (var hotkeyAction in hotkeysWaitingForRelease)
                        {
                            try
                            {
                                hotkeyAction.Value.Action.Invoke();
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex, $"An error occurred trying to trigger action for on-release hotkey '{hotkeyAction.Key}'");
                            }
                        }
                    }
                }

                foreach (var keyName in onReleaseHotkeys.Keys)
                {
                    onReleaseHotkeys[keyName].CurrentlyHeld = false;
                }

                foreach (var keyName in quickCastHotkeys.Keys)
                {
                    quickCastHotkeys[keyName].CurrentlyHeld = false;
                }


                logger.Info($"logging key up - lparam: {e.lParam} - key: {e.keyName} - all keys down: {string.Join('-', pressedKeys)} - without modifiers: {string.Join('-', nonModifierKeys)}");

                stopwatch.Stop();
                logger.Info($"processing KeyUp event took: {stopwatch.ElapsedMilliseconds} ms");
            }
        }

        private void UpdateNewlyPressedKeys(KeyboardHook.HotkeyServiceHookEventArgs e)
        {
            if (!pressedKeys.Contains(e.keyName))
            {
                pressedKeys.Add(e.keyName);
            }

            if (!nonModifierKeys.Contains(e.keyName) && !keyboardHook.ModifierKeys.Contains(e.keyName))
            {
                nonModifierKeys.Add(e.keyName);
            }
        }

        private void UpdateLiftedKeys(KeyboardHook.HotkeyServiceHookEventArgs e)
        {
            if (pressedKeys.Contains(e.keyName))
            {
                pressedKeys.Remove(e.keyName);
            }

            if (nonModifierKeys.Contains(e.keyName))
            {
                nonModifierKeys.Remove(e.keyName);
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

        public void AddOrUpdateQuickCastHotkey(string settingString, Contracts.HotkeyAction hotkeyAction)
        {
            if (this.quickCastHotkeys.ContainsKey(settingString))
            {
                this.quickCastHotkeys[settingString].Action = hotkeyAction.Action;
            }
            else
            {
                this.quickCastHotkeys.Add(settingString, hotkeyAction);
            }
        }

        public void AddOrUpdateOnReleaseHotkey(string settingString, Contracts.HotkeyAction hotkeyAction)
        {
            if (this.onReleaseHotkeys.ContainsKey(settingString))
            {
                this.onReleaseHotkeys[settingString].Action = hotkeyAction.Action;
            }
            else
            {
                this.onReleaseHotkeys.Add(settingString, hotkeyAction);
            }
        }
    }


}
