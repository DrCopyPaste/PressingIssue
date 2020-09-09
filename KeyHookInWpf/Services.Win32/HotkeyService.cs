using Services.Contracts;
using System;
using System.Collections.Generic;

namespace Services.Win32
{
    public class HotkeyService : IHotkeyService, IDisposable
    {
        private readonly NLog.Logger logger = null;

        private List<Action> hotkeyActions = null;
        private KeyboardHook keyboardHook = null;
        private HashSet<string> pressedKeys = null;
        private HashSet<string> nonModifierKeys = null;

        public HotkeyService()
        {
            logger = NLog.LogManager.GetCurrentClassLogger();
            keyboardHook = new KeyboardHook();

            this.hotkeyActions = new List<Action>()
            {
                () => logger.Info($"[{keyboardHook.Guid.ToString()}] logging a key action")
            };

            keyboardHook.KeyEvent += Mahook_KeyEvent;
            keyboardHook.Start();

            pressedKeys = new HashSet<string>();
            nonModifierKeys = new HashSet<string>();
        }

        public void Dispose()
        {
            keyboardHook.Stop();
        }

        private void Mahook_KeyEvent(object sender, KeyboardHook.HotkeyServiceHookEventArgs e)
        {
            if (e.keyDown)
            {
                if (!pressedKeys.Contains(e.keyName))
                {
                    pressedKeys.Add(e.keyName);
                }

                if (!nonModifierKeys.Contains(e.keyName) && !keyboardHook.ModifierKeys.Contains(e.keyName))
                {
                    nonModifierKeys.Add(e.keyName);
                }

                logger.Info($"logging key down - lparam: {e.lParam} - key: {e.keyName} - all keys down: {string.Join('-', pressedKeys)} - without modifiers: {string.Join('-', nonModifierKeys)}");
            }
            else if (e.keyUp)
            {
                if (pressedKeys.Contains(e.keyName))
                {
                    pressedKeys.Remove(e.keyName);
                }

                if (nonModifierKeys.Contains(e.keyName))
                {
                    nonModifierKeys.Remove(e.keyName);
                }

                logger.Info($"logging key up - lparam: {e.lParam} - key: {e.keyName} - all keys down: {string.Join('-', pressedKeys)} - without modifiers: {string.Join('-', nonModifierKeys)}");
            }
        }

        public void AddKeyDownAction(Action keyDownAction)
        {
            this.hotkeyActions.Add(keyDownAction);
        }

        public void AddKeyUpAction(Action keyUpAction)
        {
            this.hotkeyActions.Add(keyUpAction);
        }

        public string GetCurrentKeysPressed()
        {
            throw new NotImplementedException();
        }

        public void StartHook()
        {
            throw new NotImplementedException();
        }

        public void StopHook()
        {
            throw new NotImplementedException();
        }

        

        
    }

   
}
