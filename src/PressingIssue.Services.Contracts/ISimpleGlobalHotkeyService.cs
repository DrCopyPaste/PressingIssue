using Services.Contracts.Events;
using System;

namespace Services.Contracts
{
    public interface ISimpleGlobalHotkeyService
    {
        bool ProcessingHotkeys { get; set; }
        bool Running { get; }

        // you can attach to this to do additional actions on key down/up
        // but it is not needed for processing hotkeys
        event EventHandler<SimpleGlobalHotkeyServiceEventArgs> KeyEvent;

        void Start(bool processingHotkeys = true);
        void Stop();

        void AddOrUpdateQuickCastHotkey(string settingString, Action hotkeyAction);
        
        void AddOrUpdateOnReleaseHotkey(string settingString, Action hotkeyAction);
    }
}
