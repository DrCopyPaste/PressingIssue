namespace PressingIssue.Services.Contracts.Events
{
    public class SimpleGlobalHotkeyServiceEventArgs
    {
        public bool KeyDown { get; private set; }
        public bool KeyUp { get; private set; }
        public Keys Key { get; private set; }
        public string AsSettingString { get; private set; }
        public bool IsWinPressed { get; private set; }
        public bool IsAltPressed { get; private set; }
        public bool IsCtrlPressed { get; private set; }
        public bool IsShiftPressed { get; private set; }

        public SimpleGlobalHotkeyServiceEventArgs(bool keyDown, Keys key, bool isWinPressed, bool isAltPressed, bool isCtrlPressed, bool isShiftPressed)
        {
            this.Key = key;

            this.KeyDown = keyDown;
            this.KeyUp = !keyDown;

            this.IsWinPressed = isWinPressed;
            this.IsAltPressed = isAltPressed;
            this.IsCtrlPressed = isCtrlPressed;
            this.IsShiftPressed = isShiftPressed;
        }
    }
}
