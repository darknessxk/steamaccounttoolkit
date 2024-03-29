﻿using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SteamAccountToolkit.Classes
{
    public static class NtApi
    {
        public delegate bool EnumWindowProc(IntPtr hwnd, IntPtr lParam);

        public const bool EnumWindowsContinueEnumerating = true;
        public const bool EnumWindowsStopEnumerating = false;

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string szClass,
            string szWindow);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowProc cb, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int GetWindowTextLength(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hwnd, StringBuilder szText, int maxCount);

        [DllImport("user32.dll")]
        public static extern int GetClassName(IntPtr hwnd, StringBuilder szClass, int maxCount);
    }
}