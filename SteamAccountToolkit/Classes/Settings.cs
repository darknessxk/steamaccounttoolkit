using System;
using System.Runtime.Serialization;

namespace SteamAccountToolkit.Classes
{
    [Serializable]
    public class Settings
    {
        [DataMember] public string ThemeAccent;
        [DataMember] public string ThemeColor;
    }
}