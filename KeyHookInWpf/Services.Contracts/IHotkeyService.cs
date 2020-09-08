/*

at first we only add hotkeys that trigger "on release" (in key up event, meaning we have to memorize and update currently pressed keys via key down)
other hotkey modes could be:
    - on key down (with repeat interval?)
    - 
 resources:
        https://docs.microsoft.com/en-us/archive/blogs/toub/low-level-keyboard-hook-in-c
        https://docs.microsoft.com/en-us/archive/msdn-magazine/2002/october/cutting-edge-windows-hooks-in-the-net-framework

    
see http://pinvoke.net/default.aspx/Enums/HookType.html
see https://www.pinvoke.net/default.aspx/user32.setwindowshookex
see https://www.pinvoke.net/default.aspx/user32.unhookwindowshookex
 
 */

using System;

namespace Services.Contracts
{
    public interface IHotkeyService
    {
        void PauseActions();
        void ResumeActions();

        void AddKeyDownAction();
        
        void AddKeyUpAction();

        string GetCurrentKeysPressed();
    }
}
