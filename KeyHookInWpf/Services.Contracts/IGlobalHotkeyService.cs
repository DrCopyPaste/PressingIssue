using System;

namespace Services.Contracts
{
    public interface IGlobalHotkeyService
    {
        void StartHook();
        void StopHook();

        void AddOrUpdateQuickCastHotkey(string settingString, Action hotkeyAction);
        
        void AddOrUpdateOnReleaseHotkey(string settingString, Action hotkeyAction);

        string GetCurrentKeysPressed();
    }
}
