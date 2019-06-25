using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using SteamAccountToolkit.Classes;
using SteamAccountToolkit.Classes.Security;

namespace SteamAccountToolkit
{
    public static class Globals
    {
        #region Logger

        public static ObservableCollection<Logger.LogItem> LoggerCollection = new ObservableCollection<Logger.LogItem>();
        public static ObservableCollection<Logger.LogItem> LoggerCollectionProp { get; } = LoggerCollection;
        public static Logger Log { get; } = new Logger(ref LoggerCollection);
        #endregion

        public static BitmapImage DefaultImage;
        public static Encoding Encoder => Encoding.UTF8;
        public static byte[] CurrentDataPackVersion => new byte[] { 0x1, 0x0, 0x0, 0x01 };

#if DEBUG
        public static string Version { get; } = @"0.03 (DEBUG VERSION)";
#else
        public static string Version { get; } = $@"0.03";
#endif

        public static Storage Storage { get; } = new Storage();
        public static Settings SettingsManager { get; } = new Settings(Storage);
        public static Settings.SettingsOptions Settings { get; set; }
        public static Cryptography Cryptography { get; } = new Cryptography();
        public static Steam Steam { get; } = new Steam(Storage, Cryptography);
        public static ObservableCollection<SteamUser> Users => Steam.Users;
        public static bool IsAppRunning;
    }
}