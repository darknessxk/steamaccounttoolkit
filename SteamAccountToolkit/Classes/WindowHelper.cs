using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WinSendKeys = System.Windows.Forms.SendKeys;

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

                return NtApi.EnumWindowsContinueEnumerating;
            }, IntPtr.Zero);

            return winList ?? Enumerable.Empty<WinHandle>();
        }

        public static WinHandle FindWindow(Predicate<WinHandle> pred)
        {
            if (pred == null)
                throw new ArgumentNullException(nameof(pred));

            var ret = WinHandle.Invalid;

            NtApi.EnumWindows((ptr, lp) =>
            {
                var win = new WinHandle(ptr);
                if (pred.Invoke(win))
                {
                    ret = win;
                    return NtApi.EnumWindowsStopEnumerating;
                }
                return NtApi.EnumWindowsContinueEnumerating;
            }, IntPtr.Zero);

            return ret;
        }

        //some shitty extensions pls

        public static string GetWindowText(this WinHandle win)
        {
            var size = NtApi.GetWindowTextLength(win.Handle);

            if(size > 0)
            {
                var sb = new StringBuilder(size + 1); // +1 [size+1] = '\0';
                NtApi.GetWindowText(win.Handle, sb, sb.Capacity);
                return sb.ToString();
            }

            return string.Empty;
        }

        public static string GetClassName(this WinHandle win)
        {
            var limit = 255;
            var aSize = 0;
            StringBuilder sb;
            do
            {
                sb = new StringBuilder(limit);
                aSize = NtApi.GetClassName(win.Handle, sb, sb.Capacity);
                aSize *= 2;
            } while (aSize == limit - 1);

            return sb.ToString();
        }

        public static WinHandle SendKeys(this WinHandle win, string text, bool sendCharbyChar = false, int interval = 10)
        {
            if(sendCharbyChar)
                foreach (var c in text)
                {
                    System.Threading.Thread.Sleep(interval);
                    win.SendKey(c);
                }
            else
            {
                if (NtApi.GetForegroundWindow() != win.Handle)
                    NtApi.SetForegroundWindow(win.Handle);
                WinSendKeys.SendWait(text);
            }

            return win;
        }

        public static WinHandle SendKey(this WinHandle win, char c)
        {
            if (NtApi.GetForegroundWindow() != win.Handle)
                NtApi.SetForegroundWindow(win.Handle);
            WinSendKeys.SendWait(c.ToString());
            return win;
        }

        public static bool IsVisible(this WinHandle win)
        {
            return NtApi.IsWindowVisible(win.Handle);
        }
    }
}