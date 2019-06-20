using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using SteamAccountToolkit.Classes.Security;

namespace SteamAccountToolkit.Classes
{
    public class Steam
    {


        private Storage Storage { get; }
        private Cryptography Crypto { get; }

        public Steam(Storage storage, Cryptography crypto)
        {
            Crypto = crypto;
            Storage = storage;

            if (!Directory.Exists(UsersPath))
                Directory.CreateDirectory(UsersPath);
        }

        public ObservableCollection<SteamUser> Users { get; } = new ObservableCollection<SteamUser>();

        public bool IsLoading { get; set; } = true;
        public bool IsPathPending => string.IsNullOrEmpty(GetSteamPath());

        private static string FileExtension => ".satuser";
        private static readonly byte[] EncryptionMarker = { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0xDE, 0xAD, 0xBB, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };
        public string UsersPath => Path.Combine(Storage.FolderPath, "Users");

        public bool IsOnMainWindow()
        {
            return GetSteamMainWindow() != IntPtr.Zero;
        }

        public bool IsOnSteamGuard()
        {
            return GetSteamLoginWindow() != IntPtr.Zero;
        }

        public bool IsOnLogin()
        {
            return GetSteamLoginWindow() != IntPtr.Zero;
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
                if (x.EndsWith(FileExtension)) LoadUserFromFile(x);
            });
        }

        private static bool UserIsEncrypted(IReadOnlyList<byte> target) => Utils.PatternFinder(target, EncryptionMarker) > -1;

        public void LoadUserFromFile(string filePath, byte[] encKey = null)
        {
            var pak = Storage.Load(filePath, Globals.Encoder.GetBytes("SteamUser"));
            if (pak.Data.Length <= 0) return;

            if (UserIsEncrypted(pak.Data))
            {
                if (encKey == null)
                    throw new Exception("Missing encryption key");

                Crypto.Decrypt(pak.Data, encKey, out var decBytes);
                pak.Data = decBytes;
            }

            using (var ms = new MemoryStream(pak.Data))
            {
                IFormatter f = new BinaryFormatter();

                var objData = f.Deserialize(ms);

                if (objData is SteamUser.SerializableSteamUser user)
                    AddNewUser(new SteamUser(user));
            }
        }

        public void AddNewUser(SteamUser user)
        {
            Users.Add(user);
            user.Initialize();
        }

        public void SaveUser(SteamUser user, byte[] encKey = null)
        {
            if (user == null) return;
            var fileName = $"{user.SteamId}-{user.PersonaName}{FileExtension}";

            DeleteUser(user, false); // in case of a possible updating action lol

            using (var ms = new MemoryStream())
            {
                IFormatter f = new BinaryFormatter();

                f.Serialize(ms, user.User);
                var data = ms.ToArray();
                if(encKey != null)
                    Crypto.Encrypt(data, encKey, out data);

                Storage.Save(Path.Combine(UsersPath, fileName), data, Globals.Encoder.GetBytes("SteamUser"));
            }
        }

        public void DeleteUser(SteamUser user)
        {
            DeleteUser(user, true);
        }

        public void DeleteUser(SteamUser user, bool deleteFromList)
        {
            if (deleteFromList)
                Utils.InvokeDispatcherIfRequired(() => Users.Remove(user));

            var fileName = $"{user.SteamId}-{user.PersonaName}{FileExtension}";

            if (File.Exists(Path.Combine(UsersPath, fileName)))
                File.Delete(Path.Combine(UsersPath, fileName));
        }

        public string GetSteamPath()
        {
            var rKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser,
                Environment.Is64BitProcess ? RegistryView.Registry64 : RegistryView.Registry32);

            try
            {
                rKey = rKey.OpenSubKey(@"Software\\Valve\\Steam");
                if (rKey != null) return $"{rKey.GetValue("SteamExe")}";
            }
            catch
            {
                return string.Empty;
            }

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
            var window = WindowHelper.FindWindow(w =>
                w.GetWindowText().StartsWith("Steam Guard —") || w.GetWindowText().StartsWith("Steam Guard -"));
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

                    while ((attempts < attemptsLimit) & (pId == 0))
                    {
                        NtApi.GetWindowThreadProcessId(sgHwnd, out pId);
                        Thread.Sleep(250);
                        attempts++;
                    }

                    new WinHandle(sgHwnd).SendKeys(login.SteamGuard.GenerateSteamGuardCode());

                    NtApi.SetForegroundWindow(sgHwnd);

                    SendKeys.SendWait("{ENTER}");

                    Thread.Sleep(
                        5000); // i think 5 seconds is enough hmm (3 seconds sometimes send another key command)
                    if (!IsOnSteamGuard())
                        break;
                }

                //continue to main window gg!
            });
        }
    }
}