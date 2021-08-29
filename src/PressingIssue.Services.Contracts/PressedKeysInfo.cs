namespace PressingIssue.Services.Contracts
{
    public struct PressedKeysInfo
    {
        public Keys Keys { get; set; }
        public bool IsWinPressed { get; set; }
        public bool IsAltPressed { get; set; }
        public bool IsCtrlPressed { get; set; }
        public bool IsShiftPressed { get; set; }

        public static PressedKeysInfo Empty => new PressedKeysInfo() { Keys = Keys.None, IsAltPressed = false, IsCtrlPressed = false, IsShiftPressed = false, IsWinPressed = false };
    }
}
