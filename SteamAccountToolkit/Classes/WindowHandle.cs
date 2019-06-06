using System;

namespace SteamAccountToolkit.Classes
{
    public class WinHandle
    {
        public IntPtr Handle { get; private set; }

        public WinHandle(IntPtr hwnd)
        {
            Handle = hwnd;
        }

        public static WinHandle Invalid => new WinHandle(IntPtr.Zero);
        public bool IsValid => Handle != IntPtr.Zero;
    }
}
