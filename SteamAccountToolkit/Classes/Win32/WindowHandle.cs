using System;

namespace SteamAccountToolkit.Classes
{
    public class WinHandle
    {
        public WinHandle(IntPtr hwnd)
        {
            Handle = hwnd;
        }

        public IntPtr Handle { get; }

        public static WinHandle Invalid => new WinHandle(IntPtr.Zero);
        public bool IsValid => Handle != IntPtr.Zero;
    }
}