using System;
using System.Runtime.Serialization;

namespace SteamAccountToolkit.Classes
{
    [Serializable]
    public class Settings
    {
        [DataMember]
        public string ThemeColor;

        [DataMember]
        public string ThemeAccent;
    }
}
