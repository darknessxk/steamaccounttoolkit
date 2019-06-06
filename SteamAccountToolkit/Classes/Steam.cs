using System;
using System.Diagnostics;
using Microsoft.Win32;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Linq;

namespace SteamAccountToolkit.Classes
{
    public class Steam
    {
        public bool IsOnMainWindow() => GetSteamMainWindow() != IntPtr.Zero;
        public bool IsOnSteamGuard() => GetSteamLoginWindow() != IntPtr.Zero;
        public bool IsOnLogin() => GetSteamLoginWindow() != IntPtr.Zero;
        private Storage Storage { get; }

        public ObservableCollection<SteamUser> Users { get; } = new ObservableCollection<SteamUser>();

        public bool IsLoading { get; set; } = true;
        public bool IsPathPending => string.IsNullOrEmpty(GetSteamPath());

        private static string FileExtension => ".satuser";
        private static Encoding Encoder => Encoding.UTF8;
        public string UsersPath => Path.Combine(Storage.FolderPath, "Users");

        public Steam(Storage storage)
        {
            Storage = storage;

            if (!Directory.Exists(UsersPath))
                Directory.CreateDirectory(UsersPath);
        }

        public void Initialize()
        {
            LoadUserList();
            IsLoading = false;
        }

        public void LoadUserList()
        {
            Directory.GetFiles(UsersPath).ToList().ForEach(x =>
            {
                if (x.EndsWith(FileExtension))
                {
                    LoadUserFromFile(x);
                }
            });
        }

        public void LoadUserFromFile(string filePath)
        {
            var pak = Storage.Load(filePath, Storage.FileHashAlgo.ComputeHash(Encoder.GetBytes("SteamUser")));
            if (pak.Data.Length <= 0) return;
            using (var ms = new MemoryStream(pak.Data))
            {
                IFormatter f = new BinaryFormatter();
                object b = null;

                b = f.Deserialize(ms);

                if (b is SteamUser.SerializableSteamUser user)
                    AddNewUser(new SteamUser(user));
            }
        }

        public void AddNewUser(SteamUser user)
        {
            Users.Add(user);
            user.Initialize();
        }

        public void SaveUser(SteamUser user)
        {
            if (user == null) return;
            var hashValue = Storage.HashAlgo.ComputeHash(Encoder.GetBytes(user.Username));
            var fileName = $"{BitConverter.ToString(hashValue)}{FileExtension}".Replace("-", string.Empty);

            DeleteUser(user); // in case of a possible updating action lol

            using (var ms = new MemoryStream())
            {
                IFormatter f = new BinaryFormatter();

                f.Serialize(ms, user.User);


                Storage.Save(Path.Combine(UsersPath, fileName), new Storage.DataPack(Storage.FileHashAlgo)
                {
                    Data = ms.ToArray(),
                    Header = new Storage.DataHeader(Storage.FileHashAlgo.ComputeHash(Encoder.GetBytes("SteamUser")))
                });
            }
        }

        public void DeleteUser(SteamUser user)
        {
            Utils.InvokeDispatcherIfRequired(() => Users.Remove(user));

            var hashValue = Storage.HashAlgo.ComputeHash(Encoder.GetBytes(user.Username.ToString()));
            var fileName = $"{BitConverter.ToString(hashValue)}{FileExtension}".Replace("-", string.Empty);

            if (File.Exists(Path.Combine(UsersPath, fileName)))
                File.Delete(Path.Combine(UsersPath, fileName));
        }

        public string GetSteamPath()
        {
            var rKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, Environment.Is64BitProcess ? RegistryView.Registry64 : RegistryView.Registry32);

            try
            {
                rKey = rKey.OpenSubKey(@"Software\\Valve\\Steam");
                if (rKey != null) return $"{rKey.GetValue("SteamExe")}";
            }
            catch { return string.Empty; }
            return string.Empty;
        }

        public IntPtr GetSteamWarningWindow()
        {
            var hwnd = NtApi.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "vguiPopupWindow", @"Steam —");
            if (hwnd == IntPtr.Zero)
                hwnd = NtApi.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "vguiPopupWindow", "Steam - ");
            return hwnd;
        }

        public IntPtr GetSteamLoginWindow()
        {
            return NtApi.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "vguiPopupWindow", "Steam Login");
        }

        public IntPtr GetSteamMainWindow()
        {
            var steamHwnd = NtApi.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "vguiPopupWindow", "Steam");
            if (steamHwnd == IntPtr.Zero) return IntPtr.Zero;
            var cef = NtApi.FindWindowEx(steamHwnd, IntPtr.Zero, "CefBrowserWindow", "");
            return cef;
        }

        public IntPtr GetSteamGuardWindow()
        {
            var window = WindowHelper.FindWindow(w => w.GetWindowText().StartsWith("Steam Guard —") || w.GetWindowText().StartsWith("Steam Guard -"));
            return window.Handle;
        }

        public void Shutdown()
        {
            Process.Start(new ProcessStartInfo(GetSteamPath(), "-shutdown"));
        }

        public void DoLogin(SteamUser login)
        {
            Task.Run(() =>
            {
                var calledShutdown = false;
                while (IsOnSteamGuard() || IsOnMainWindow() || IsOnLogin())
                {
                    if (!calledShutdown)
                    {
                        calledShutdown = true;
                        Shutdown();
                    }
                    Thread.Sleep(10);
                }

                Process.Start(new ProcessStartInfo(GetSteamPath(), $"-login {login.Username} {login.Password}"));

                while (!IsOnLogin() || Process.GetProcessesByName("Steam").Length == 0)
                    Thread.Sleep(10);

                while (IsOnSteamGuard() && !IsOnMainWindow())
                {
                    Thread.Sleep(250); // CPU Saving and window timing
                    //Do steam guard job here
                    var sgHwnd = GetSteamGuardWindow();

                    if (NtApi.GetForegroundWindow() != sgHwnd)
                        NtApi.SetForegroundWindow(sgHwnd);

                    var pId = 0;
                    var attempts = 0;
                    var attemptsLimit = 10;

                    while (attempts < attemptsLimit & pId == 0)
                    {
                        NtApi.GetWindowThreadProcessId(sgHwnd, out pId);
                        Thread.Sleep(250);
                        attempts++;
                    }

                    new WinHandle(sgHwnd).SendKeys(login.SteamGuard.GenerateSteamGuardCode());

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
