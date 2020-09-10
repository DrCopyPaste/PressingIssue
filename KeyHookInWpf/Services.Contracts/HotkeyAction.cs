using System;
using System.Collections.Generic;
using System.Text;

namespace Services.Contracts
{
    public class HotkeyAction
    {
        // this holds the currently held state so the key does not repeat action as long as it is pressed
        public bool CurrentlyHeld { get; set; } = false;
        public Action Action { get; set; }
    }
}
