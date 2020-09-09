/*

at first we only add hotkeys that trigger "on release" (in key up event, meaning we have to memorize and update currently pressed keys via key down)
in a later stage one could also allow hotkeys to trigger "on key down" AND "on release" for instance: start capturing on keydown and stop capturing on same key up

 resources:
http://www.dylansweb.com/2014/10/low-level-global-keyboard-hook-sink-in-c-net/



        https://docs.microsoft.com/en-us/archive/blogs/toub/low-level-keyboard-hook-in-c
        https://docs.microsoft.com/en-us/archive/msdn-magazine/2002/october/cutting-edge-windows-hooks-in-the-net-framework

https://www.pinvoke.net/default.aspx/Delegates/HookProc.html    
http://pinvoke.net/default.aspx/Enums/HookType.html
https://www.pinvoke.net/default.aspx/user32.setwindowshookex
https://www.pinvoke.net/default.aspx/user32.unhookwindowshookex
https://www.pinvoke.net/default.aspx/user32.callnexthookex

https://www.pinvoke.net/default.aspx/kernel32/GetModuleHandle.html

https://www.pinvoke.net/default.aspx/user32.getkeystate

http://pinvoke.net/default.aspx/Constants.WM
 */


using Services.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Services.Win32
{
    public class HotkeyService : IHotkeyService, IDisposable
    {
        // https://www.pinvoke.net/default.aspx/user32.getkeystate
        private const int KEY_PRESSED = 0x8000;

        // http://pinvoke.net/default.aspx/Constants.WM
        private const UInt32 WM_ACTIVATE = 0x0006;
        private const UInt32 WM_ACTIVATEAPP = 0x001C;
        private const UInt32 WM_AFXFIRST = 0x0360;
        private const UInt32 WM_AFXLAST = 0x037F;
        private const UInt32 WM_APP = 0x8000;
        private const UInt32 WM_ASKCBFORMATNAME = 0x030C;
        private const UInt32 WM_CANCELJOURNAL = 0x004B;
        private const UInt32 WM_CANCELMODE = 0x001F;
        private const UInt32 WM_CAPTURECHANGED = 0x0215;
        private const UInt32 WM_CHANGECBCHAIN = 0x030D;
        private const UInt32 WM_CHANGEUISTATE = 0x0127;
        private const UInt32 WM_CHAR = 0x0102;
        private const UInt32 WM_CHARTOITEM = 0x002F;
        private const UInt32 WM_CHILDACTIVATE = 0x0022;
        private const UInt32 WM_CLEAR = 0x0303;
        private const UInt32 WM_CLOSE = 0x0010;
        private const UInt32 WM_COMMAND = 0x0111;
        private const UInt32 WM_COMPACTING = 0x0041;
        private const UInt32 WM_COMPAREITEM = 0x0039;
        private const UInt32 WM_CONTEXTMENU = 0x007B;
        private const UInt32 WM_COPY = 0x0301;
        private const UInt32 WM_COPYDATA = 0x004A;
        private const UInt32 WM_CREATE = 0x0001;
        private const UInt32 WM_CTLCOLORBTN = 0x0135;
        private const UInt32 WM_CTLCOLORDLG = 0x0136;
        private const UInt32 WM_CTLCOLOREDIT = 0x0133;
        private const UInt32 WM_CTLCOLORLISTBOX = 0x0134;
        private const UInt32 WM_CTLCOLORMSGBOX = 0x0132;
        private const UInt32 WM_CTLCOLORSCROLLBAR = 0x0137;
        private const UInt32 WM_CTLCOLORSTATIC = 0x0138;
        private const UInt32 WM_CUT = 0x0300;
        private const UInt32 WM_DEADCHAR = 0x0103;
        private const UInt32 WM_DELETEITEM = 0x002D;
        private const UInt32 WM_DESTROY = 0x0002;
        private const UInt32 WM_DESTROYCLIPBOARD = 0x0307;
        private const UInt32 WM_DEVICECHANGE = 0x0219;
        private const UInt32 WM_DEVMODECHANGE = 0x001B;
        private const UInt32 WM_DISPLAYCHANGE = 0x007E;
        private const UInt32 WM_DRAWCLIPBOARD = 0x0308;
        private const UInt32 WM_DRAWITEM = 0x002B;
        private const UInt32 WM_DROPFILES = 0x0233;
        private const UInt32 WM_ENABLE = 0x000A;
        private const UInt32 WM_ENDSESSION = 0x0016;
        private const UInt32 WM_ENTERIDLE = 0x0121;
        private const UInt32 WM_ENTERMENULOOP = 0x0211;
        private const UInt32 WM_ENTERSIZEMOVE = 0x0231;
        private const UInt32 WM_ERASEBKGND = 0x0014;
        private const UInt32 WM_EXITMENULOOP = 0x0212;
        private const UInt32 WM_EXITSIZEMOVE = 0x0232;
        private const UInt32 WM_FONTCHANGE = 0x001D;
        private const UInt32 WM_GETDLGCODE = 0x0087;
        private const UInt32 WM_GETFONT = 0x0031;
        private const UInt32 WM_GETHOTKEY = 0x0033;
        private const UInt32 WM_GETICON = 0x007F;
        private const UInt32 WM_GETMINMAXINFO = 0x0024;
        private const UInt32 WM_GETOBJECT = 0x003D;
        private const UInt32 WM_GETTEXT = 0x000D;
        private const UInt32 WM_GETTEXTLENGTH = 0x000E;
        private const UInt32 WM_HANDHELDFIRST = 0x0358;
        private const UInt32 WM_HANDHELDLAST = 0x035F;
        private const UInt32 WM_HELP = 0x0053;
        private const UInt32 WM_HOTKEY = 0x0312;
        private const UInt32 WM_HSCROLL = 0x0114;
        private const UInt32 WM_HSCROLLCLIPBOARD = 0x030E;
        private const UInt32 WM_ICONERASEBKGND = 0x0027;
        private const UInt32 WM_IME_CHAR = 0x0286;
        private const UInt32 WM_IME_COMPOSITION = 0x010F;
        private const UInt32 WM_IME_COMPOSITIONFULL = 0x0284;
        private const UInt32 WM_IME_CONTROL = 0x0283;
        private const UInt32 WM_IME_ENDCOMPOSITION = 0x010E;
        private const UInt32 WM_IME_KEYDOWN = 0x0290;
        private const UInt32 WM_IME_KEYLAST = 0x010F;
        private const UInt32 WM_IME_KEYUP = 0x0291;
        private const UInt32 WM_IME_NOTIFY = 0x0282;
        private const UInt32 WM_IME_REQUEST = 0x0288;
        private const UInt32 WM_IME_SELECT = 0x0285;
        private const UInt32 WM_IME_SETCONTEXT = 0x0281;
        private const UInt32 WM_IME_STARTCOMPOSITION = 0x010D;
        private const UInt32 WM_INITDIALOG = 0x0110;
        private const UInt32 WM_INITMENU = 0x0116;
        private const UInt32 WM_INITMENUPOPUP = 0x0117;
        private const UInt32 WM_INPUTLANGCHANGE = 0x0051;
        private const UInt32 WM_INPUTLANGCHANGEREQUEST = 0x0050;
        private const UInt32 WM_KEYDOWN = 0x0100;
        private const UInt32 WM_KEYFIRST = 0x0100;
        private const UInt32 WM_KEYLAST = 0x0108;
        private const UInt32 WM_KEYUP = 0x0101;
        private const UInt32 WM_KILLFOCUS = 0x0008;
        private const UInt32 WM_LBUTTONDBLCLK = 0x0203;
        private const UInt32 WM_LBUTTONDOWN = 0x0201;
        private const UInt32 WM_LBUTTONUP = 0x0202;
        private const UInt32 WM_MBUTTONDBLCLK = 0x0209;
        private const UInt32 WM_MBUTTONDOWN = 0x0207;
        private const UInt32 WM_MBUTTONUP = 0x0208;
        private const UInt32 WM_MDIACTIVATE = 0x0222;
        private const UInt32 WM_MDICASCADE = 0x0227;
        private const UInt32 WM_MDICREATE = 0x0220;
        private const UInt32 WM_MDIDESTROY = 0x0221;
        private const UInt32 WM_MDIGETACTIVE = 0x0229;
        private const UInt32 WM_MDIICONARRANGE = 0x0228;
        private const UInt32 WM_MDIMAXIMIZE = 0x0225;
        private const UInt32 WM_MDINEXT = 0x0224;
        private const UInt32 WM_MDIREFRESHMENU = 0x0234;
        private const UInt32 WM_MDIRESTORE = 0x0223;
        private const UInt32 WM_MDISETMENU = 0x0230;
        private const UInt32 WM_MDITILE = 0x0226;
        private const UInt32 WM_MEASUREITEM = 0x002C;
        private const UInt32 WM_MENUCHAR = 0x0120;
        private const UInt32 WM_MENUCOMMAND = 0x0126;
        private const UInt32 WM_MENUDRAG = 0x0123;
        private const UInt32 WM_MENUGETOBJECT = 0x0124;
        private const UInt32 WM_MENURBUTTONUP = 0x0122;
        private const UInt32 WM_MENUSELECT = 0x011F;
        private const UInt32 WM_MOUSEACTIVATE = 0x0021;
        private const UInt32 WM_MOUSEFIRST = 0x0200;
        private const UInt32 WM_MOUSEHOVER = 0x02A1;
        private const UInt32 WM_MOUSELAST = 0x020D;
        private const UInt32 WM_MOUSELEAVE = 0x02A3;
        private const UInt32 WM_MOUSEMOVE = 0x0200;
        private const UInt32 WM_MOUSEWHEEL = 0x020A;
        private const UInt32 WM_MOUSEHWHEEL = 0x020E;
        private const UInt32 WM_MOVE = 0x0003;
        private const UInt32 WM_MOVING = 0x0216;
        private const UInt32 WM_NCACTIVATE = 0x0086;
        private const UInt32 WM_NCCALCSIZE = 0x0083;
        private const UInt32 WM_NCCREATE = 0x0081;
        private const UInt32 WM_NCDESTROY = 0x0082;
        private const UInt32 WM_NCHITTEST = 0x0084;
        private const UInt32 WM_NCLBUTTONDBLCLK = 0x00A3;
        private const UInt32 WM_NCLBUTTONDOWN = 0x00A1;
        private const UInt32 WM_NCLBUTTONUP = 0x00A2;
        private const UInt32 WM_NCMBUTTONDBLCLK = 0x00A9;
        private const UInt32 WM_NCMBUTTONDOWN = 0x00A7;
        private const UInt32 WM_NCMBUTTONUP = 0x00A8;
        private const UInt32 WM_NCMOUSEHOVER = 0x02A0;
        private const UInt32 WM_NCMOUSELEAVE = 0x02A2;
        private const UInt32 WM_NCMOUSEMOVE = 0x00A0;
        private const UInt32 WM_NCPAINT = 0x0085;
        private const UInt32 WM_NCRBUTTONDBLCLK = 0x00A6;
        private const UInt32 WM_NCRBUTTONDOWN = 0x00A4;
        private const UInt32 WM_NCRBUTTONUP = 0x00A5;
        private const UInt32 WM_NCXBUTTONDBLCLK = 0x00AD;
        private const UInt32 WM_NCXBUTTONDOWN = 0x00AB;
        private const UInt32 WM_NCXBUTTONUP = 0x00AC;
        private const UInt32 WM_NCUAHDRAWCAPTION = 0x00AE;
        private const UInt32 WM_NCUAHDRAWFRAME = 0x00AF;
        private const UInt32 WM_NEXTDLGCTL = 0x0028;
        private const UInt32 WM_NEXTMENU = 0x0213;
        private const UInt32 WM_NOTIFY = 0x004E;
        private const UInt32 WM_NOTIFYFORMAT = 0x0055;
        private const UInt32 WM_NULL = 0x0000;
        private const UInt32 WM_PAINT = 0x000F;
        private const UInt32 WM_PAINTCLIPBOARD = 0x0309;
        private const UInt32 WM_PAINTICON = 0x0026;
        private const UInt32 WM_PALETTECHANGED = 0x0311;
        private const UInt32 WM_PALETTEISCHANGING = 0x0310;
        private const UInt32 WM_PARENTNOTIFY = 0x0210;
        private const UInt32 WM_PASTE = 0x0302;
        private const UInt32 WM_PENWINFIRST = 0x0380;
        private const UInt32 WM_PENWINLAST = 0x038F;
        private const UInt32 WM_POWER = 0x0048;
        private const UInt32 WM_POWERBROADCAST = 0x0218;
        private const UInt32 WM_PRINT = 0x0317;
        private const UInt32 WM_PRINTCLIENT = 0x0318;
        private const UInt32 WM_QUERYDRAGICON = 0x0037;
        private const UInt32 WM_QUERYENDSESSION = 0x0011;
        private const UInt32 WM_QUERYNEWPALETTE = 0x030F;
        private const UInt32 WM_QUERYOPEN = 0x0013;
        private const UInt32 WM_QUEUESYNC = 0x0023;
        private const UInt32 WM_QUIT = 0x0012;
        private const UInt32 WM_RBUTTONDBLCLK = 0x0206;
        private const UInt32 WM_RBUTTONDOWN = 0x0204;
        private const UInt32 WM_RBUTTONUP = 0x0205;
        private const UInt32 WM_RENDERALLFORMATS = 0x0306;
        private const UInt32 WM_RENDERFORMAT = 0x0305;
        private const UInt32 WM_SETCURSOR = 0x0020;
        private const UInt32 WM_SETFOCUS = 0x0007;
        private const UInt32 WM_SETFONT = 0x0030;
        private const UInt32 WM_SETHOTKEY = 0x0032;
        private const UInt32 WM_SETICON = 0x0080;
        private const UInt32 WM_SETREDRAW = 0x000B;
        private const UInt32 WM_SETTEXT = 0x000C;
        private const UInt32 WM_SETTINGCHANGE = 0x001A;
        private const UInt32 WM_SHOWWINDOW = 0x0018;
        private const UInt32 WM_SIZE = 0x0005;
        private const UInt32 WM_SIZECLIPBOARD = 0x030B;
        private const UInt32 WM_SIZING = 0x0214;
        private const UInt32 WM_SPOOLERSTATUS = 0x002A;
        private const UInt32 WM_STYLECHANGED = 0x007D;
        private const UInt32 WM_STYLECHANGING = 0x007C;
        private const UInt32 WM_SYNCPAINT = 0x0088;
        private const UInt32 WM_SYSCHAR = 0x0106;
        private const UInt32 WM_SYSCOLORCHANGE = 0x0015;
        private const UInt32 WM_SYSCOMMAND = 0x0112;
        private const UInt32 WM_SYSDEADCHAR = 0x0107;
        private const UInt32 WM_SYSKEYDOWN = 0x0104;
        private const UInt32 WM_SYSKEYUP = 0x0105;
        private const UInt32 WM_TCARD = 0x0052;
        private const UInt32 WM_TIMECHANGE = 0x001E;
        private const UInt32 WM_TIMER = 0x0113;
        private const UInt32 WM_UNDO = 0x0304;
        private const UInt32 WM_UNINITMENUPOPUP = 0x0125;
        private const UInt32 WM_USER = 0x0400;
        private const UInt32 WM_USERCHANGED = 0x0054;
        private const UInt32 WM_VKEYTOITEM = 0x002E;
        private const UInt32 WM_VSCROLL = 0x0115;
        private const UInt32 WM_VSCROLLCLIPBOARD = 0x030A;
        private const UInt32 WM_WINDOWPOSCHANGED = 0x0047;
        private const UInt32 WM_WINDOWPOSCHANGING = 0x0046;
        private const UInt32 WM_WININICHANGE = 0x001A;
        private const UInt32 WM_XBUTTONDBLCLK = 0x020D;
        private const UInt32 WM_XBUTTONDOWN = 0x020B;
        private const UInt32 WM_XBUTTONUP = 0x020C;

        private IntPtr currentHook;

        private Guid guid = Guid.NewGuid();
        private readonly NLog.Logger logger = null;

        private List<Action> hotkeyActions = null;
        private HookProc myCallbackDelegate = null;

        public HotkeyService()
        {
            logger = NLog.LogManager.GetCurrentClassLogger();

            this.hotkeyActions = new List<Action>()
            {
                () => logger.Info(string.Format("[{0}] logging a key action", guid.ToString()))
            };

            this.myCallbackDelegate = new HookProc(this.MyCallbackFunction);

            logger.Info(string.Format("[{0}] Hotkey service started", guid.ToString()));

            using (Process process = Process.GetCurrentProcess())
            using (ProcessModule module = process.MainModule)
            {
                IntPtr hModule = GetModuleHandle(module.ModuleName);
                currentHook = SetWindowsHookEx(HookType.WH_KEYBOARD_LL, this.myCallbackDelegate, hModule, 0);
            }
        }

        public void Dispose()
        {
            UnhookWindowsHookEx(currentHook);

            logger.Info(string.Format("[{0}] Hotkey service shut down", guid.ToString()));
        }

        // https://www.pinvoke.net/default.aspx/Delegates/HookProc.html
        private delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);

        // https://www.pinvoke.net/default.aspx/user32.getkeystate
        [DllImport("user32.dll")]
        static extern short GetKeyState(VirtualKeyStates nVirtKey);

        // see https://www.pinvoke.net/default.aspx/kernel32/GetModuleHandle.html
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        // see https://www.pinvoke.net/default.aspx/user32.setwindowshookex
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetWindowsHookEx(HookType hookType, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        // see https://www.pinvoke.net/default.aspx/user32.unhookwindowshookex
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool UnhookWindowsHookEx(IntPtr hhk);

        // see https://www.pinvoke.net/default.aspx/user32.callnexthookex
        [DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        public void AddKeyDownAction(Action keyDownAction)
        {
            this.hotkeyActions.Add(keyDownAction);
        }

        public void AddKeyUpAction(Action keyUpAction)
        {
            this.hotkeyActions.Add(keyUpAction);
        }

        public string GetCurrentKeysPressed()
        {
            throw new NotImplementedException();
        }

        public void StartHook()
        {
            throw new NotImplementedException();
        }

        public void StopHook()
        {
            throw new NotImplementedException();
        }

        private IntPtr MyCallbackFunction(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code < 0)
            {
                //you need to call CallNextHookEx without further processing
                //and return the value returned by CallNextHookEx
                return CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
            }

            // WM_KEYDOWN / WM_KEYUP capture most key events
            // WM_SYSKEYDOWN / WM_SYSKEYUP is needed to capture events where ALT is helt down plus other keys

            if (wParam == (IntPtr)WM_KEYDOWN)
            {
                logger.Info(string.Format("[{0}] logging WM_KEYDOWN, wparam: {1} lparam: {2} Keyboard.Modifiers {3}", guid.ToString(), wParam, lParam, System.Windows.Input.Keyboard.Modifiers.ToString()));
            }
            else if (wParam == (IntPtr)WM_SYSKEYDOWN)
            {
                logger.Info(string.Format("[{0}] logging WM_SYSKEYDOWN, wparam: {1} lparam: {2} Keyboard.Modifiers {3}", guid.ToString(), wParam, lParam, System.Windows.Input.Keyboard.Modifiers.ToString()));
            }
            else if (wParam == (IntPtr)WM_KEYUP)
            {
                logger.Info(string.Format("[{0}] logging WM_KEYUP, wparam: {1} lparam: {2} Keyboard.Modifiers {3}", guid.ToString(), wParam, lParam, System.Windows.Input.Keyboard.Modifiers.ToString()));
            }
            else if (wParam == (IntPtr)WM_SYSKEYUP)
            {
                logger.Info(string.Format("[{0}] logging WM_SYSKEYUP, wparam: {1} lparam: {2} Keyboard.Modifiers {3}", guid.ToString(), wParam, lParam, System.Windows.Input.Keyboard.Modifiers.ToString()));
            }

            if (hotkeyActions.Count > 0)
            {
                // hotkeyActions[0].Invoke();
            }

            // we can convert the 2nd parameter (the key code) to a System.Windows.Forms.Keys enum constant
            //Keys keyPressed = (Keys)wParam.ToInt32();
            //Console.WriteLine(keyPressed);
            //return the value returned by CallNextHookEx
            return CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
        }
    }

    // see http://pinvoke.net/default.aspx/Enums/HookType.html
    public enum HookType : int
    {
        WH_JOURNALRECORD = 0,
        WH_JOURNALPLAYBACK = 1,
        WH_KEYBOARD = 2,
        WH_GETMESSAGE = 3,
        WH_CALLWNDPROC = 4,
        WH_CBT = 5,
        WH_SYSMSGFILTER = 6,
        WH_MOUSE = 7,
        WH_HARDWARE = 8,
        WH_DEBUG = 9,
        WH_SHELL = 10,
        WH_FOREGROUNDIDLE = 11,
        WH_CALLWNDPROCRET = 12,
        WH_KEYBOARD_LL = 13,
        WH_MOUSE_LL = 14
    }

    // https://www.pinvoke.net/default.aspx/user32.getkeystate
    enum VirtualKeyStates : int
    {
        VK_LBUTTON = 0x01,
        VK_RBUTTON = 0x02,
        VK_CANCEL = 0x03,
        VK_MBUTTON = 0x04,
        //
        VK_XBUTTON1 = 0x05,
        VK_XBUTTON2 = 0x06,
        //
        VK_BACK = 0x08,
        VK_TAB = 0x09,
        //
        VK_CLEAR = 0x0C,
        VK_RETURN = 0x0D,
        //
        VK_SHIFT = 0x10,
        VK_CONTROL = 0x11,
        VK_MENU = 0x12,
        VK_PAUSE = 0x13,
        VK_CAPITAL = 0x14,
        //
        VK_KANA = 0x15,
        VK_HANGEUL = 0x15,  /* old name - should be here for compatibility */
        VK_HANGUL = 0x15,
        VK_JUNJA = 0x17,
        VK_FINAL = 0x18,
        VK_HANJA = 0x19,
        VK_KANJI = 0x19,
        //
        VK_ESCAPE = 0x1B,
        //
        VK_CONVERT = 0x1C,
        VK_NONCONVERT = 0x1D,
        VK_ACCEPT = 0x1E,
        VK_MODECHANGE = 0x1F,
        //
        VK_SPACE = 0x20,
        VK_PRIOR = 0x21,
        VK_NEXT = 0x22,
        VK_END = 0x23,
        VK_HOME = 0x24,
        VK_LEFT = 0x25,
        VK_UP = 0x26,
        VK_RIGHT = 0x27,
        VK_DOWN = 0x28,
        VK_SELECT = 0x29,
        VK_PRINT = 0x2A,
        VK_EXECUTE = 0x2B,
        VK_SNAPSHOT = 0x2C,
        VK_INSERT = 0x2D,
        VK_DELETE = 0x2E,
        VK_HELP = 0x2F,
        //
        VK_LWIN = 0x5B,
        VK_RWIN = 0x5C,
        VK_APPS = 0x5D,
        //
        VK_SLEEP = 0x5F,
        //
        VK_NUMPAD0 = 0x60,
        VK_NUMPAD1 = 0x61,
        VK_NUMPAD2 = 0x62,
        VK_NUMPAD3 = 0x63,
        VK_NUMPAD4 = 0x64,
        VK_NUMPAD5 = 0x65,
        VK_NUMPAD6 = 0x66,
        VK_NUMPAD7 = 0x67,
        VK_NUMPAD8 = 0x68,
        VK_NUMPAD9 = 0x69,
        VK_MULTIPLY = 0x6A,
        VK_ADD = 0x6B,
        VK_SEPARATOR = 0x6C,
        VK_SUBTRACT = 0x6D,
        VK_DECIMAL = 0x6E,
        VK_DIVIDE = 0x6F,
        VK_F1 = 0x70,
        VK_F2 = 0x71,
        VK_F3 = 0x72,
        VK_F4 = 0x73,
        VK_F5 = 0x74,
        VK_F6 = 0x75,
        VK_F7 = 0x76,
        VK_F8 = 0x77,
        VK_F9 = 0x78,
        VK_F10 = 0x79,
        VK_F11 = 0x7A,
        VK_F12 = 0x7B,
        VK_F13 = 0x7C,
        VK_F14 = 0x7D,
        VK_F15 = 0x7E,
        VK_F16 = 0x7F,
        VK_F17 = 0x80,
        VK_F18 = 0x81,
        VK_F19 = 0x82,
        VK_F20 = 0x83,
        VK_F21 = 0x84,
        VK_F22 = 0x85,
        VK_F23 = 0x86,
        VK_F24 = 0x87,
        //
        VK_NUMLOCK = 0x90,
        VK_SCROLL = 0x91,
        //
        VK_OEM_NEC_EQUAL = 0x92,   // '=' key on numpad
                                   //
        VK_OEM_FJ_JISHO = 0x92,   // 'Dictionary' key
        VK_OEM_FJ_MASSHOU = 0x93,   // 'Unregister word' key
        VK_OEM_FJ_TOUROKU = 0x94,   // 'Register word' key
        VK_OEM_FJ_LOYA = 0x95,   // 'Left OYAYUBI' key
        VK_OEM_FJ_ROYA = 0x96,   // 'Right OYAYUBI' key
                                 //
        VK_LSHIFT = 0xA0,
        VK_RSHIFT = 0xA1,
        VK_LCONTROL = 0xA2,
        VK_RCONTROL = 0xA3,
        VK_LMENU = 0xA4,
        VK_RMENU = 0xA5,
        //
        VK_BROWSER_BACK = 0xA6,
        VK_BROWSER_FORWARD = 0xA7,
        VK_BROWSER_REFRESH = 0xA8,
        VK_BROWSER_STOP = 0xA9,
        VK_BROWSER_SEARCH = 0xAA,
        VK_BROWSER_FAVORITES = 0xAB,
        VK_BROWSER_HOME = 0xAC,
        //
        VK_VOLUME_MUTE = 0xAD,
        VK_VOLUME_DOWN = 0xAE,
        VK_VOLUME_UP = 0xAF,
        VK_MEDIA_NEXT_TRACK = 0xB0,
        VK_MEDIA_PREV_TRACK = 0xB1,
        VK_MEDIA_STOP = 0xB2,
        VK_MEDIA_PLAY_PAUSE = 0xB3,
        VK_LAUNCH_MAIL = 0xB4,
        VK_LAUNCH_MEDIA_SELECT = 0xB5,
        VK_LAUNCH_APP1 = 0xB6,
        VK_LAUNCH_APP2 = 0xB7,
        //
        VK_OEM_1 = 0xBA,   // ';:' for US
        VK_OEM_PLUS = 0xBB,   // '+' any country
        VK_OEM_COMMA = 0xBC,   // ',' any country
        VK_OEM_MINUS = 0xBD,   // '-' any country
        VK_OEM_PERIOD = 0xBE,   // '.' any country
        VK_OEM_2 = 0xBF,   // '/?' for US
        VK_OEM_3 = 0xC0,   // '`~' for US
                           //
        VK_OEM_4 = 0xDB,  //  '[{' for US
        VK_OEM_5 = 0xDC,  //  '\|' for US
        VK_OEM_6 = 0xDD,  //  ']}' for US
        VK_OEM_7 = 0xDE,  //  ''"' for US
        VK_OEM_8 = 0xDF,
        //
        VK_OEM_AX = 0xE1,  //  'AX' key on Japanese AX kbd
        VK_OEM_102 = 0xE2,  //  "<>" or "\|" on RT 102-key kbd.
        VK_ICO_HELP = 0xE3,  //  Help key on ICO
        VK_ICO_00 = 0xE4,  //  00 key on ICO
                           //
        VK_PROCESSKEY = 0xE5,
        //
        VK_ICO_CLEAR = 0xE6,
        //
        VK_PACKET = 0xE7,
        //
        VK_OEM_RESET = 0xE9,
        VK_OEM_JUMP = 0xEA,
        VK_OEM_PA1 = 0xEB,
        VK_OEM_PA2 = 0xEC,
        VK_OEM_PA3 = 0xED,
        VK_OEM_WSCTRL = 0xEE,
        VK_OEM_CUSEL = 0xEF,
        VK_OEM_ATTN = 0xF0,
        VK_OEM_FINISH = 0xF1,
        VK_OEM_COPY = 0xF2,
        VK_OEM_AUTO = 0xF3,
        VK_OEM_ENLW = 0xF4,
        VK_OEM_BACKTAB = 0xF5,
        //
        VK_ATTN = 0xF6,
        VK_CRSEL = 0xF7,
        VK_EXSEL = 0xF8,
        VK_EREOF = 0xF9,
        VK_PLAY = 0xFA,
        VK_ZOOM = 0xFB,
        VK_NONAME = 0xFC,
        VK_PA1 = 0xFD,
        VK_OEM_CLEAR = 0xFE
    }
}
