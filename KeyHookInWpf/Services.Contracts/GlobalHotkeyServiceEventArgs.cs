using System;
using System.Collections.Generic;
using System.Text;

namespace Services.Contracts
{
    public class GlobalHotkeyServiceEventArgs
    {
        public bool KeyDown { get; private set; }
        public bool KeyUp { get; private set; }

        public List<string> PressedKeys { get; private set; }
        public HashSet<string> PressedNonModifierKeys { get; private set; }

        public string AsSettingString { get; private set; }

        public GlobalHotkeyServiceEventArgs(bool keyDown, List<string> pressedKeys, HashSet<string> pressedNonModifierKeys, string settingString)
        {
            this.KeyDown = keyDown;
            this.KeyUp = !keyDown;

            this.PressedKeys = pressedKeys;
            this.PressedNonModifierKeys = pressedNonModifierKeys;

            this.AsSettingString = settingString;
        }
    }
}
