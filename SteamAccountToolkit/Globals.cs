using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace SteamAccountToolkit
{
    public static class Globals
    {
        public static BitmapImage DefaultImage;

#if DEBUG
        public static string Version { get; } = $@"0.02d DEV";
#else
        public static string Version { get; } = $@"0.02d RC";
#endif

        public static Classes.Storage Storage { get; } = new Classes.Storage();
        public static Classes.Steam Steam { get; } = new Classes.Steam(Storage);
        public static ObservableCollection<Classes.SteamUser> Users => Steam.Users;
        public static bool IsAppRunning;
    }
}
