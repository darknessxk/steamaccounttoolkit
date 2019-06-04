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
        public static Classes.Steam Steam { get; } = new Classes.Steam();
        public static ObservableCollection<Classes.SteamUser> Users => Steam.Users;
        public static bool IsAppRunning;
    }
}
