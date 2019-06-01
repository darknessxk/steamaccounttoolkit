using System;
using System.Diagnostics;
using Microsoft.Win32;
using System.Threading;
using SendKeys = System.Windows.Forms.SendKeys;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Linq;

namespace SteamAutoLogin
{
    public static class SteamDesktopApi
    {
        public static bool IsOnMainWindow() => GetSteamMainWindow() != IntPtr.Zero;
        public static bool IsOnSteamGuard() => GetSteamLoginWindow() != IntPtr.Zero;
        public static bool IsOnLogin() => GetSteamLoginWindow() != IntPtr.Zero;

        public static string GetSteamPath()
        {
            RegistryKey rKey;
            if (Environment.Is64BitProcess)
                rKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
            else
                rKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32);

            try
            {
                rKey = rKey.OpenSubKey(@"Software\\Valve\\Steam");
                return $"{rKey.GetValue("SteamExe")}";
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string GetProfileIconUrl(LoginData login)
        {
            if(login.SteamId64 != string.Empty)
            {
                var profileUrl = $"https://steamcommunity.com/profiles/{login.SteamId64}";

                try
                {
                    HtmlDocument doc = new HtmlWeb().Load(new Uri(profileUrl));
                    return doc.DocumentNode.Descendants().Where(n => n.HasClass("playerAvatarAutoSizeInner")).First().FirstChild.GetAttributeValue("src", null);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return string.Empty;
        }

        public static IntPtr GetSteamWarningWindow()
        {
            var hwnd = NtApi.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "vguiPopupWindow", @"Steam —");
            if (hwnd == IntPtr.Zero)
                hwnd = NtApi.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "vguiPopupWindow", "Steam - ");
            return hwnd;
        }

        public static IntPtr GetSteamLoginWindow()
        {
            return NtApi.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "vguiPopupWindow", "Steam Login");
        }

        public static IntPtr GetSteamMainWindow()
        {
            var SteamHwnd = NtApi.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "vguiPopupWindow", "Steam");
            if (SteamHwnd != IntPtr.Zero)
            {
                var cef = NtApi.FindWindowEx(SteamHwnd, IntPtr.Zero, "CefBrowserWindow", "");
                return cef;
            }
            return IntPtr.Zero;
        }

        public static IntPtr GetSteamGuardWindow()
        {
            //WindowHelper.FindWindow(w => w.GetWindowText().StartsWith("Steam Guard —")) || WindowHelper.FindWindow(w => w.GetWindowText().StartsWith("Steam Guard -"))
            var window = WindowHelper.FindWindow(w => w.GetWindowText().StartsWith("Steam Guard —") || w.GetWindowText().StartsWith("Steam Guard -"));
            return window.Handle;
        }

        public static void QuitSteam()
        {
            Process.Start(new ProcessStartInfo(GetSteamPath(), "-shutdown"));
        }

        public static void SteamLogin(LoginData login)
        {
            var task = Task.Run(() =>
            {
                Process.Start(new ProcessStartInfo(GetSteamPath(), $"-login {login.User} {login.Pass.ToString()}"));

                while (!IsOnSteamGuard() && !IsOnMainWindow())
                {
                    Thread.Sleep(10);
                }

                SteamAuth.SteamGuardAccount acc = new SteamAuth.SteamGuardAccount { SharedSecret = login.SteamGuardPrivateKey.ToString() };

                while (IsOnSteamGuard() && !IsOnMainWindow())
                {
                    Thread.Sleep(250); // CPU Saving and window timing
                    //Do steam guard job here
                    var sgHwnd = GetSteamGuardWindow();

                    if (NtApi.GetForegroundWindow() != sgHwnd)
                        NtApi.SetForegroundWindow(sgHwnd);

                    int pId = 0;
                    int attempts = 0;
                    int attemptsLimit = 10;

                    while(attempts < attemptsLimit & pId == 0)
                    {
                        NtApi.GetWindowThreadProcessId(sgHwnd, out pId);
                        Thread.Sleep(250);
                        attempts++;
                    }

                    // Second Check
                    if (NtApi.GetForegroundWindow() != sgHwnd)
                        NtApi.SetForegroundWindow(sgHwnd);

                    foreach (char c in acc.GenerateSteamGuardCode())
                    {
                        NtApi.SetForegroundWindow(sgHwnd);
                        Thread.Sleep(5);
                        SendKeys.SendWait(c.ToString());
                    }

                    NtApi.SetForegroundWindow(sgHwnd);

                    SendKeys.SendWait("{ENTER}");

                    Thread.Sleep(5000); // i think 5 seconds is enough hmm (3 seconds sometimes send another key command)

                    if (!IsOnSteamGuard())
                        break;
                }

                //continue to main window gg!
            });
        }
    }
}
