using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamAccountToolkit
{
    public static class Globals
    {
        public static string Version { get; } = "0.02";
        public static Classes.Storage Storage { get; } = new Classes.Storage();
        public static Classes.Steam Steam { get; } = new Classes.Steam(Storage);
        public static ObservableCollection<Classes.SteamUser> Users => Steam.Users;
        public static bool IsAppRunning;
    }
}
