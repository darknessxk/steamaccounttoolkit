using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamAccountToolkit
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
