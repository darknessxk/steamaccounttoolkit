using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
