﻿/*

at first we only add hotkeys that trigger "on release" (in key up event, meaning we have to memorize and update currently pressed keys via key down)
in a later stage one could also allow hotkeys to trigger "on key down" AND "on release" for instance: start capturing on keydown and stop capturing on same key up

 resources:
        https://docs.microsoft.com/en-us/archive/blogs/toub/low-level-keyboard-hook-in-c
        https://docs.microsoft.com/en-us/archive/msdn-magazine/2002/october/cutting-edge-windows-hooks-in-the-net-framework

https://www.pinvoke.net/default.aspx/Delegates/HookProc.html    
http://pinvoke.net/default.aspx/Enums/HookType.html
https://www.pinvoke.net/default.aspx/user32.setwindowshookex
https://www.pinvoke.net/default.aspx/user32.unhookwindowshookex
https://www.pinvoke.net/default.aspx/user32.callnexthookex

https://www.pinvoke.net/default.aspx/kernel32/GetModuleHandle.html
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

            if (hotkeyActions.Count > 0)
            {
                hotkeyActions[0].Invoke();
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
}