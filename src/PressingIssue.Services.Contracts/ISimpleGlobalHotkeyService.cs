﻿using System;
using PressingIssue.Services.Contracts.Events;

namespace PressingIssue.Services.Contracts
{
    public interface ISimpleGlobalHotkeyService : IBasicGlobalHotkeyService
    {
        // you can attach to this to do additional actions on key down/up
        // but it is not needed for processing hotkeys
        event EventHandler<SimpleGlobalHotkeyServiceEventArgs> KeyEvent;

        void AddOrUpdateQuickCastHotkey(Keys key, bool isWinPressed, bool isAltPressed, bool isCtrlPressed, bool isShiftPressed, Action hotkeyAction);

        void AddOrUpdateOnReleaseHotkey(Keys key, bool isWinPressed, bool isAltPressed, bool isCtrlPressed, bool isShiftPressed, Action hotkeyAction);

        string GetPressedKeysAsSetting(Keys key, bool isWinPressed, bool isAltPressed, bool isCtrlPressed, bool isShiftPressed);
    }
}
