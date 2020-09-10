using System;

namespace Services.Contracts
{
    public interface IHotkeyService
    {
        void StartHook();
        void StopHook();

        void AddOrUpdateQuickCastHotkey(string settingString, Action hotkeyAction);
        
        void AddOrUpdateOnReleaseHotkey(string settingString, Action hotkeyAction);

        string GetCurrentKeysPressed();
    }
}
