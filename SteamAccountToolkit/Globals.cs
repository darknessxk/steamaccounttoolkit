using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Media.Imaging;
using SteamAccountToolkit.Classes;
using SteamAccountToolkit.Classes.Security;

namespace SteamAccountToolkit
{
    public static class Globals
    {
        public static BitmapImage DefaultImage;
        public static Encoding Encoder => Encoding.UTF8;
        public static byte[] CurrentDataPackVersion => new byte[] { 0x1, 0x0, 0x0, 0x01 };

#if DEBUG
        public static string Version { get; } = @"0.02d (DEBUG VERSION)";
#else
        public static string Version { get; } = $@"0.02d";
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