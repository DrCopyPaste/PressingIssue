using System;

namespace Services.Contracts
{
    public interface IHotkeyService
    {
        void StartHook();
        void StopHook();

        void AddOrUpdateQuickCastHotkey(string settingString, HotkeyAction hotkeyAction);
        
        void AddOrUpdateOnReleaseHotkey(string settingString, HotkeyAction hotkeyAction);

        string GetCurrentKeysPressed();
    }
}
