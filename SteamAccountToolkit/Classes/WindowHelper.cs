using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamAccountToolkit.Classes
{
    public static class WindowHelper
    {
        public static WinHandle GetForegroundWindow() => new WinHandle(NtApi.GetForegroundWindow());
        public static IEnumerable<WinHandle> FindWindows(Predicate<WinHandle> pred)
        {
            if (pred == null)
                throw new ArgumentNullException(nameof(pred));

            List<WinHandle> winList = null;

            NtApi.EnumWindows((ptr, lp) =>
            {
                var win = new WinHandle(ptr);

                if(pred.Invoke(win))
                {
                    if(winList == null)
                        winList = new List<WinHandle>();
                    winList.Add(win);
                }

                return NtApi.EnumWindows_ContinueEnumerating;
            }, IntPtr.Zero);

            return winList ?? Enumerable.Empty<WinHandle>();
        }

        public static WinHandle FindWindow(Predicate<WinHandle> pred)
        {
            if (pred == null)
                throw new ArgumentNullException(nameof(pred));

            WinHandle ret = WinHandle.Invalid;

            NtApi.EnumWindows((ptr, lp) =>
            {
                var win = new WinHandle(ptr);
                if (pred.Invoke(win))
                {
                    ret = win;
                    return NtApi.EnumWindows_StopEnumerating;
                }
                return NtApi.EnumWindows_ContinueEnumerating;
            }, IntPtr.Zero);

            return ret;
        }

        //some shitty extensions pls

        public static string GetWindowText(this WinHandle win)
        {
            int size = NtApi.GetWindowTextLength(win.Handle);

            if(size > 0)
            {
                StringBuilder sb = new StringBuilder(size + 1); // +1 [size+1] = '\0';
                NtApi.GetWindowText(win.Handle, sb, sb.Capacity);
                return sb.ToString();
            }

            return string.Empty;
        }

        public static string GetClassName(this WinHandle win)
        {
            int limit = 255;
            int aSize = 0;
            StringBuilder sb;
            do
            {
                sb = new StringBuilder(limit);
                aSize = NtApi.GetClassName(win.Handle, sb, sb.Capacity);
                aSize *= 2;
            } while (aSize == limit - 1);

            return sb.ToString();
        }

        public static bool IsVisible(this WinHandle win)
        {
            return NtApi.IsWindowVisible(win.Handle);
        }
    }
}