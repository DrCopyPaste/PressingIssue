using System;

namespace Services.Contracts
{
    public interface IHotkeyService
    {
        void StartHook();
        void StopHook();

        void AddKeyDownAction(Action keyDownAction);
        
        void AddKeyUpAction(Action keyUpAction);

        string GetCurrentKeysPressed();
    }
}
