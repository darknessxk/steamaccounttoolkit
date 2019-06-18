using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using SteamAccountToolkit.Classes;

namespace SteamAccountToolkit
{
    public static class Globals
    {
        public static BitmapImage DefaultImage;

#if DEBUG
        public static string Version { get; } = @"0.02d DEV";
#else
        public static string Version { get; } = $@"0.02d RC";
#endif

        public static Storage Storage { get; } = new Storage();
        public static Steam Steam { get; } = new Steam(Storage);
        public static ObservableCollection<SteamUser> Users => Steam.Users;
        public static bool IsAppRunning;
    }
}